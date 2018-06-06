using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpXmlToGhostMigrator.Converters
{
    class ListItemConverter : MarkdownConverterBase
    {
        public override string[] SupportedTags => new string[] { "ul", "ol", "li" };

        private Stack<string> _lists = new Stack<string>();
        public override void CloseNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            if (node.Name == "li") return;

            if (_lists.Peek() != node.Name)
            {
                throw new InvalidOperationException("Mismatch!");
            }

            _lists.Pop();
            bodyBuilder.AppendLine();
        }

        public override bool OpenNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            string content = string.Empty;
            switch (node.Name)
            {
                case "ul":
                case "ol":
                    _lists.Push(node.Name);

                    //if (_lists.Count == 1)
                    {
                        bodyBuilder.AppendLine();
                    }


                    // this is a weird hack, but we're doing it because HTML is weird
                    // but we only care about the li elements, and we treat the rest as problematic (non-existant)
                    // and if we loose content, so be it...
                    foreach (var child in node.Children)
                    {
                        if (child.GetNameSafely() != "li")
                        {
                            Console.WriteLine("!! Content ignored: " + child.ToString());
                            continue;
                        }

                        content = MarkdownVisitor.Process(child).Trim('\r', '\n', ' ');

                        if (content.IndexOf('\n') >= 0)
                        {
                            // we need to add a \t at every new line
                            content = content.Replace("\n", "\n\t");
                        }

                        bodyBuilder.AppendFormat("{0} {1}", node.Name == "ul" ? "*" : "1.", content).AppendLine();
                    }

                    return false;

                case "li":
                    content = MarkdownVisitor.Process(node).Trim('\n', ' ');

                    bodyBuilder.AppendFormat("{0} {1}", _lists.Peek() == "ul" ? "*" : "1.", content).AppendLine();
                    return false;
            }
            return true;
        }
    }
}
