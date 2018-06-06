using System;
using System.Collections.Generic;
using System.Text;

namespace WpXmlToGhostMigrator.Converters
{
    class ListItemConverter : MarkdownConverterBase
    {
        public override string[] SupportedTags => new string[] { "ul", "ol", "li" };

        public override void CloseNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            throw new NotImplementedException();
        }

        public override bool OpenNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            switch(node.Name)
            {
                case "ul":
                case "ol":

                    break;

                case "li":

                    break;
            }
            return true;
        }
    }
}
