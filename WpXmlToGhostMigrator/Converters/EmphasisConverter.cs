using System;
using System.Text;

namespace WpXmlToGhostMigrator.Converters
{
    class EmphasisConverter : MarkdownConverterBase
    {
        public override string[] SupportedTags => new string[] { "em", "b", "strong", "i" };

        public override void CloseNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            // sometimes the emphasis ends with a whitespace character which is slightly 
            // annoying, but valid in HTML - so we move it after our node ending
            var suffix = bodyBuilder.ToString().Substring(bodyBuilder.Length - 1);
            if (String.IsNullOrWhiteSpace(suffix))
            {
                bodyBuilder.Remove(bodyBuilder.Length - 1, 1);
            }
            else { suffix = null; }

            Process(node, bodyBuilder);

            if(suffix != null)
            {
                bodyBuilder.Append(suffix);
            }
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
