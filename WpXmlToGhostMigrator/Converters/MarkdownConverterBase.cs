using System;
using System.Collections.Generic;
using System.Text;

namespace WpXmlToGhostMigrator.Converters
{
    public abstract class MarkdownConverterBase
    {

        public abstract string[] SupportedTags { get; }

        /// <summary>
        /// Writes the opening of the node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>Return true if the caller is expected to handle children separately. </returns>
        public abstract bool OpenNode(HtmlParser.Node node, StringBuilder bodyBuilder);

        public abstract void CloseNode(HtmlParser.Node node, StringBuilder bodyBuilder);
    }
}
