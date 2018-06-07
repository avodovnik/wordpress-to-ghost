using System;
using System.Collections.Generic;
using System.Text;

namespace WpXmlToGhostMigrator.Converters
{
    public class HardBreakConverter : MarkdownConverterBase
    {
        public override string[] SupportedTags => new string[] { "br", "p" };

        public override void CloseNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
        }

        public override bool OpenNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            bodyBuilder.AppendLine().AppendLine(); 

            // this should never actually happen, but hey...
            return true;
        }
    }
}
