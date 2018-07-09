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
                    //case '[': // the [ is a wordpress directive, but it's essentially the same code
                        // this is a bit horrible, but should actually work
                        if (insideComment)
                            goto default;

                        // if we have any buffer
                        if (!String.IsNullOrEmpty(buffer))
                        {
                            // it's likely text, so we should just assign it to the current node
                            if (!String.IsNullOrWhiteSpace(buffer))
                            {
                                nodeStack.Peek().AddChild(new TextNode(buffer));
                            }

                            buffer = String.Empty;
                        }

                        // mark that we're inside a token
                        insideToken = true;
                        break;

                    case '>':
                    //case ']':
                        if (insideComment)
                        {
                            if (buffer.EndsWith("--"))
                            {
                                // we were clearly in a comment node
                                // which ends now 
                                insideComment = false;
                                insideToken = false;
                                nodeStack.Peek().AddChild(new CommentNode(buffer.Trim().TrimEnd('-')));

                                buffer = String.Empty;
                            }
                            else
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
                            var node = NodeFactory.Create(buffer, c);
                            buffer = String.Empty;

                            if (node == null)
                                continue;

                            // depending on what that node was, let's do something with it
                            switch (node.Type)
                            {
                                case Node.NodeType.Opening:
                                    nodeStack.Push(node);
                                    break;
                                case Node.NodeType.Closing:
                                    var stacked = nodeStack.Pop();
                                    // TODO: when the node is updated, update this :) 
                                    if (stacked.Name != node.Name)
                                    {
                                        throw new Exception("Node mismatch: " + node.Name);
                                    }

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
                        if (prevChar == '-')
                        {
                            // check if the buffer equals comment
                            if (buffer == "!-" && insideToken)
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

            // if stuff left on buffer, means we have additional text
            // if we have any buffer
            if (!String.IsNullOrEmpty(buffer))
            {
                // it's likely text, so we should just assign it to the current node
                nodeStack.Peek().AddChild(new TextNode(buffer));
                buffer = String.Empty;
            }

            if (nodeStack.Count != 1)
            {
                // cleanup the nodes, if possible,
                // but make sure the last one is always the document
                while (nodeStack.Count > 1)
                {
                    var leftover = nodeStack.Pop();
                    if (IsCleanable(leftover))
                    {
                        nodeStack.Peek().AddChild(leftover);
                    }
                }

                if (nodeStack.Count != 1)
                    throw new Exception("Somehow nodes were left on the stack and unclosed.");
            }

            return nodeStack.Pop() as HtmlDocument;
        }

        private static bool IsCleanable(Node node)
        {
            // if the tag is something like p, we can clean it up fairly easily, 
            // because we just assume we can close it and put to the parent
            switch (node.Name)
            {
                case "p":
                    return true;
                default:
                    return false;
            }
        }

        private static class NodeFactory
        {
            public static Node Create(string buffer, char tokenChar)
            {
                buffer = (buffer ?? string.Empty).Trim('\r', '\n', ' ');

                var assumedType = Node.NodeType.Opening;

                // if the node is a Wordpress directive ([), we can assume the 
                // self closing tag
                if (tokenChar == '[' || tokenChar == ']')
                    assumedType = Node.NodeType.Directive;
                else if (buffer.StartsWith('/'))
                    assumedType = Node.NodeType.Closing;
                else if (buffer.EndsWith('/'))
                    assumedType = Node.NodeType.SelfClosing;

                // TODO: hack
                if (buffer.StartsWith("img") || buffer.StartsWith("br"))
                {
                    // images are a special case, and we can actually treat them as self-closing
                    assumedType = Node.NodeType.SelfClosing;
                }

                var name = buffer.Trim('/');
                int i = name.IndexOf(" ");
                var attributes = new Dictionary<string, string>();

                if (i > 0)
                {
                    name = name.Substring(0, i);

                    // let's parse attributes
                    var attString = buffer.Substring(i);
                    attributes = ParseAttributes(attString);
                }


                return new Node()
                {
                    Type = assumedType,
                    Name = name, // TODO: TEMP
                    Attributes = attributes
                };
            }

            private static Dictionary<string, string> ParseAttributes(string str)
            {
                var attributes = new Dictionary<string, string>();

                str = (str ?? string.Empty).Trim();

                string buffer = String.Empty;
                // TODO: we're double parsing this, so think about optimising

                var name = string.Empty;
                var value = string.Empty;
                var insideAttribute = false;

                foreach (var c in str)
                {
                    switch (c)
                    {
                        case '=':
                            if (insideAttribute)
                                goto default;

                            name = buffer.Trim();
                            buffer = string.Empty;

                            break;
                        case '"':
                            if (insideAttribute)
                            {
                                value = buffer;

                                // add the value to the dictionary
                                if (attributes.ContainsKey(name))
                                {
                                    attributes[name] = String.Join(',', attributes[name], value);
                                }
                                else
                                {
                                    attributes.Add(name, value);
                                }

                                buffer = string.Empty;
                                name = string.Empty;

                            }
                            else
                            {
                                // don't really have to do anything
                            }

                            insideAttribute = !insideAttribute;
                            break;
                        default:
                            buffer += c;
                            break;
                    }
                }

                return attributes;
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
                // split into lines
                List<string> newLines = new List<string>();
                var lines = textContent.Split('\n');
                bool prevLineIsEmpty = false;
                foreach (var line in lines)
                {
                    var lx = line.Trim('\r', '\n', '\t');
                    if (!string.IsNullOrWhiteSpace(lx))
                    {
                        prevLineIsEmpty = false;
                        newLines.Add(lx);
                    }
                    else
                    {
                        if (prevLineIsEmpty)
                        {
                            newLines.Add("\r\n");
                            prevLineIsEmpty = false; // fake it 'till we make it
                        }
                        else
                        {
                            newLines.Add("\r\n"); // TODO: testing... what this does
                            prevLineIsEmpty = true;
                        }
                    }
                }

                Text = string.Join("\r\n", newLines);
            }

            public string Text { get; }

            public override string ToString()
            {
                return Text;
            }
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
                SelfClosing,
                Directive
            }

            public void AddChild(Node node)
            {
                this._nodes.Add(node);
            }

            public string Name { get; set; }

            public Dictionary<string, string> Attributes { get; internal set; }
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
