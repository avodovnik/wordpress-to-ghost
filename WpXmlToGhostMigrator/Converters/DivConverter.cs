using System;
using System.Collections.Generic;
using System.Text;

namespace WpXmlToGhostMigrator.Converters
{
    class DivConverter : MarkdownConverterBase
    {
        public override string[] SupportedTags => new string[] { "div" };

        public override void CloseNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            bodyBuilder.AppendLine().AppendLine();
        }

        public override bool OpenNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            bodyBuilder.AppendLine().AppendLine();
            return true;
        }
    }
}
