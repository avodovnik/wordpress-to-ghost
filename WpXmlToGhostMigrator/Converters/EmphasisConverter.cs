using System;
using System.Text;

namespace WpXmlToGhostMigrator.Converters
{
    class EmphasisConverter : MarkdownConverterBase
    {
        public override string[] SupportedTags => new string[] { "em", "b", "strong", "i" };

        public override void CloseNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            Process(node, bodyBuilder);
            //bodyBuilder.Append("");
        }

        public override bool OpenNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            Process(node, bodyBuilder);

            // we need to parse any child
            return true;
        }

        private void Process(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            switch (node.GetNameSafely())
            {
                case "em":
                case "i":
                    bodyBuilder.Append("_");
                    break;

                case "b":
                case "strong":
                    bodyBuilder.Append("**");
                    break;
                default:
                    throw new NotSupportedException("EmphasisConverter was picked for a tag that isn't recognized " + node.GetNameSafely());
            }
        }
    }
}
