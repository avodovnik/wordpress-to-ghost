using System;
using System.Collections.Generic;
using System.Text;

namespace WpXmlToGhostMigrator.Converters
{
    public class CaptionConverter : MarkdownConverterBase
    {
        public override string[] SupportedTags => new string[] { "caption" };

        public override void CloseNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
        }

        public override bool OpenNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            return true;
        }
    }
}
