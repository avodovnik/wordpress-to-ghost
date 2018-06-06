using System;
using System.Collections.Generic;
using System.Text;

namespace WpXmlToGhostMigrator.Converters
{
    class ImageConverter : MarkdownConverterBase
    {
        public override string[] SupportedTags => new string[] { "img" };

        public override void CloseNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
        }

        public override bool OpenNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            var src = node.Attributes.GetValueOrDefault("src");
            if (string.IsNullOrEmpty(src)) return false;

            // ![alt text](https://github.com/adam-p/markdown-here/raw/master/src/common/images/icon48.png "Logo Title Text 1")
            bodyBuilder.AppendFormat("![{0}]({1} \"{2}\")", node.Attributes.GetValueOrDefault("alt"), src, node.Attributes.GetValueOrDefault("title"));

            return false;
        }
    }
}
