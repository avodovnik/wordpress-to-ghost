using System;
using System.Collections.Generic;
using System.Text;

namespace WpXmlToGhostMigrator.Converters
{
    public class BlockquoteConverter : MarkdownConverterBase
    {
        public override string[] SupportedTags => new string[] { "blockquote" };

        public override void CloseNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
        }

        public override bool OpenNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            var content = MarkdownVisitor.Process(node).Trim('\n', '\r', ' ');

            bodyBuilder.AppendLine().Append(">");

            content = content.Replace("\n", "\n>");

            bodyBuilder.AppendLine(content).AppendLine();

            return false;
        }
    }
}
