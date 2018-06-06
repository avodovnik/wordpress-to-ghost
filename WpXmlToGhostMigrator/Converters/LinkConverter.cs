using System;
using System.Collections.Generic;
using System.Text;

namespace WpXmlToGhostMigrator.Converters
{
    class LinkConverter : MarkdownConverterBase
    {
        public override string[] SupportedTags => new string[] { "a" };

        public override void CloseNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            bodyBuilder.Append("](").Append(node.Attributes.GetValueOrDefault("href", "")).Append(") ");
        }

        public override bool OpenNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            //[I'm an inline-style link](https://www.google.com)
            bodyBuilder.Append('[');
            return true;
        }
}
}
