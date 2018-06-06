using System;
using System.Collections.Generic;
using System.Text;

namespace WpXmlToGhostMigrator.Converters
{
    class HeaderConverter : MarkdownConverterBase
    {
        public override string[] SupportedTags => new string[] { "h1", "h2", "h3", "h4", "h5", "h6" };

        public override void CloseNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            bodyBuilder.AppendLine();
        }

        public override bool OpenNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            //var content = MarkdownVisitor.Process(node);
            var levels = Int32.Parse(node.Name.Substring(1, 1));

            bodyBuilder.AppendLine().AppendLine().Append('#', levels).Append(' ');

            return true;
        }
    }
}
