using System;
using System.Collections.Generic;
using System.Text;

namespace WpXmlToGhostMigrator
{
    public static class HtmlParser
    {
        public static HtmlDocument Parse(string content)
        {
            string buffer = String.Empty;

            var nodeStack = new Stack<Node>();

            // we always start with that
            nodeStack.Push(new HtmlDocument());

            var insideToken = false;
            var insideLiteral = false;
            var insideComment = false;

            char prevChar = char.MinValue;

            // either we're a node, or a text, parse until we figure out
            foreach (char c in content)
            {
                switch (c)
                {
                    case '<':
                        // this is a bit horrible, but should actually work
                        if (insideComment)
                            goto default;

                        // if we have any buffer
                        if (!String.IsNullOrEmpty(buffer))
                        {
                            // it's likely text, so we should just assign it to the current node
                            nodeStack.Peek().AddChild(new TextNode(buffer));
                            buffer = String.Empty;
                        }

                        // mark that we're inside a token
                        insideToken = true;
                        break;

                    case '>':
                        if(insideComment)
                        {
                            if (buffer.EndsWith("--"))
                            {
                                // we were clearly in a comment node
                                // which ends now 
                                insideComment = false;
                                insideToken = false;
                                nodeStack.Peek().AddChild(new CommentNode(buffer.Trim().TrimEnd('-')));

                                buffer = String.Empty;
                            } else
                            {
                                goto default;
                            }

                            break;
                        }
                        // when inside a token, this is a special case,
                        // so make sure we handle it
                        if (insideToken && !insideLiteral)
                        {
                            // we've reached an end of a node
                            insideToken = false;
                            var node = NodeFactory.Create(buffer);
                            buffer = String.Empty;

                            // depending on what that node was, let's do something with it
                            switch (node.Type)
                            {
                                case Node.NodeType.Opening:
                                    nodeStack.Push(node);
                                    break;
                                case Node.NodeType.Closing:
                                    var stacked = nodeStack.Pop();
                                    // TODO: when the node is updated, update this :) 
                                    //if (stacked.Name != node.Name)
                                    //{
                                    //    throw new Exception("Node mismatch: " + node.Name);
                                    //}

                                    // peek the next node (if it's root, it'll be the document, which is fine)
                                    nodeStack.Peek().AddChild(stacked);
                                    break;
                                case Node.NodeType.SelfClosing:
                                    nodeStack.Peek().AddChild(node);
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            // we're not inside a token, so this seems valid
                            buffer += c;
                        }
                        break;

                    case '"':
                        if (prevChar == '\\')
                        {
                            // escape char
                            continue;
                        }

                        if (insideToken)
                        {
                            insideLiteral = !insideLiteral;
                        }
                        goto default;

                    case '-':
                        if(prevChar == '-')
                        {
                            // check if the buffer equals comment
                            if(buffer == "!-" && insideToken)
                            {
                                // we are in a comment
                                insideComment = true;
                                buffer = String.Empty;
                                break;
                            }
                        }
                        goto default;

                    default:
                        buffer += c;
                        break;
                }

                prevChar = c;
            }

            if(nodeStack.Count != 1)
            {
                throw new Exception("Somehow nodes were left on the stack and unclosed.");
            }

            return nodeStack.Pop() as HtmlDocument;
        }

        private static class NodeFactory
        {
            public static Node Create(string buffer)
            {
                var assumedType = Node.NodeType.Opening;

                if (buffer.StartsWith('/'))
                    assumedType = Node.NodeType.Closing;
                else if (buffer.EndsWith('/'))
                    assumedType = Node.NodeType.SelfClosing;


                // TODO: hack
                if(buffer.StartsWith("img") || buffer.StartsWith("br"))
                {
                    // images are a special case, and we can actually treat them as self-closing
                    assumedType = Node.NodeType.SelfClosing;
                }

                return new Node()
                {
                    Type = assumedType,
                    Name = buffer // TODO: TEMP
                };
            }
        }

        public class CommentNode : Node
        {
            public CommentNode(string content)
            {
                Content = content;
            }

            public string Content { get; }
        }

        public class HtmlDocument : Node
        {

        }

        public class TextNode : Node
        {
            public TextNode(string textContent)
            {
                Text = textContent;
            }

            public string Text { get; }
        }

        public class Node
        {
            private List<Node> _nodes = new List<Node>();

            public IEnumerable<Node> Children { get { return _nodes; } }

            public NodeType Type { get; set; }

            public enum NodeType
            {
                Text,
                Opening,
                Closing,
                SelfClosing
            }

            public void AddChild(Node node)
            {
                this._nodes.Add(node);
            }

            public string Name { get; set; }
        }
    }
}


/*
 * <node>
 <child node/>
 <aNodeAgain>text</aNodeAgain>
</node>


-node-
  -closing
  -opening
  -self-closing

-text


node ... to stack

self closing node, 
  peek from stack, and put into children
  opening node -> push to stack
     text -> add as child to element in stack
  closing node -> pop from stack, add to element in stack

    
    -- closign and self-closign are likely the same


<!-- what happenes if this is a comment -->

    "<!--" sets a flag
    "-->" creates a comment node, and kills the flag
     
     */
