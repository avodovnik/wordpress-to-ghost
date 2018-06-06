using System;
using System.Collections.Generic;
using System.Text;
using static WpXmlToGhostMigrator.HtmlParser;

namespace WpXmlToGhostMigrator.Converters
{
    class DefaultConverter : MarkdownConverterBase
    {
        public override string[] SupportedTags => new string[] { };

        public override bool OpenNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            if(node is TextNode)
            {
                bodyBuilder.Append(node.ToString());
            }

            return true;
        }

        public override void CloseNode(Node node, StringBuilder bodyBuilder)
        {
        }
    }
}
