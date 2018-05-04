﻿using System;
using System.Collections.Generic;
using System.Text;

namespace WpXmlToGhostMigrator
{
    public static class HtmlParser
    {
        private enum State
        {
            Nowhere,
            InsideNode,
            ClosingNode
        }

        public static HtmlDocument Parse(string htmlContent)
        {
            var parserState = State.Nowhere;

            // iterate through the characters
            var document = new HtmlDocument();

            string buffer = String.Empty;
            var nodeStack = new Stack<Node>();

            foreach (char c in htmlContent)
            {
                switch (c)
                {
                    case '<':
                        // we're starting a node
                        // flush buffer 
                        FlushBuffer(buffer, document, nodeStack);
                        buffer = String.Empty;

                        break;

                    case '>':
                        // buffer is probably the contents (name, or end) of the node, we'll let it parse it (i.e. attributes)
                        // I know this is inneficient, but it was a matter of getting it done first, then optimizing

                        // there needs to be a node on the stack, otherwise, we have a problem
                        if (!nodeStack.TryPeek(out Node node))
                        {
                            throw new Exception("An end tag was encountered, but no start tag was there to put a node on the stack.");
                        }

                        // let's give the buffer to the node, to re-parse (inefficient bit), but keep the node on the stack
                        node.Parse(buffer);

                        if (node.IsClosed)
                        {
                            // since the node is closed, it needs to go to the document
                            document.AddNode(nodeStack.Pop());
                        }

                        buffer = string.Empty;
                        break;

                    default:
                        buffer += c;
                        break;
                }
            }

            return document;
        }

        private static void FlushBuffer(string buffer, HtmlDocument document, Stack<Node> nodeStack)
        {
            var textNode = new NodeTextBlock(buffer);
            // if there is a node in the node stack, that is still open, we can put it there
            if (nodeStack.TryPeek(out Node node))
            {
                // we should never encounter a tag that is closed and still on the stack, but hey...
                if (!node.IsClosed)
                {
                    node.AddChild(textNode);
                    return;
                }

                // if we do get here, should we panic?
                throw new Exception("There's a closed node on the stack - it wasn't addedd? Name: " + node.Name);
            }

            // we should probably map this as a line into markdown?
            document.AddNode(textNode);
        }

        public interface INode
        {

        }

        public class Node : INode
        {
            public List<INode> Children { get; } = new List<INode>();

            public Dictionary<string, string> Attributes { get; private set; } = new Dictionary<string, string>();

            public string Name { get; set; }

            public bool IsClosed { get; set; } = false;

            /// <summary>
            /// Parses the string between < and > and determines the node name, and attributes.
            /// </summary>
            /// <param name="nodeNameWithAttributes"></param>
            public void Parse(string nodeNameWithAttributes)
            {
                var buffer = String.Empty;


                var attributeName = String.Empty;
                var insideAttribute = false;

                if (nodeNameWithAttributes.StartsWith('/') || nodeNameWithAttributes.EndsWith('/'))
                {
                    // when we process the buffer, we need to close this tag
                    IsClosed = true;
                    nodeNameWithAttributes = nodeNameWithAttributes.Trim('/');
                }

                foreach (char c in nodeNameWithAttributes)
                {
                    switch (c)
                    {
                        case '=':
                            // if we're inside an attribute, we need to copy it
                            if (insideAttribute) { buffer += c; }

                            // left side is attribute name, next up will be attribute value
                            if (!string.IsNullOrEmpty(attributeName))
                            {
                                // something is wrong
                                throw new Exception("An = was not expected here: " + nodeNameWithAttributes);
                            }

                            break;

                        case '"':
                            if (insideAttribute)
                            {
                                // we need to wrap an attribute up now
                                insideAttribute = false;
                                if (String.IsNullOrEmpty(attributeName))
                                {
                                    throw new Exception("We got an attribute value, but no attribute name: " + nodeNameWithAttributes);
                                }

                                this.Attributes.Add(attributeName, buffer);

                                buffer = String.Empty;
                                attributeName = String.Empty;
                            }
                            else
                            {
                                // this is likely a start of an attribute
                                insideAttribute = true;
                            }
                            break;

                        default:
                            buffer += c;
                            break;
                    }
                }

                // if there's buffer, and the node name hasn't been set, it's likely
                // the string was just the node name, and we can go from there
                if (!String.IsNullOrEmpty(buffer) && String.IsNullOrEmpty(this.Name))
                {
                    this.Name = buffer.Trim();
                }

                // if it wasn't it's likely the tag was a closing one or it's a self closing tag anyway
            }

            public void AddChild(INode node)
            {
                this.Children.Add(node);
            }
        }

        private class NodeTextBlock : INode
        {
            private readonly string _content;

            public NodeTextBlock(string content)
            {
                this._content = content;
            }
        }

        public class HtmlDocument
        {
            List<INode> _nodes = new List<INode>();

            public void AddNode(INode node)
            {
                _nodes.Add(node);
            }

            public IEnumerable<INode> Nodes { get { return _nodes; } }
        }
    }




}
