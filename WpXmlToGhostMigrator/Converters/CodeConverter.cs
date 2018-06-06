using System;
using System.Collections.Generic;
using System.Text;

namespace WpXmlToGhostMigrator.Converters
{
    public class CodeConverter : MarkdownConverterBase
    {
        public override string[] SupportedTags => new string[] { "code", "pre" /* technically pre isn't code, but it is for us */ };

        // we rely on the converter being a single instance, which may not be good
        public override void CloseNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
        }

        public override bool OpenNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            var content = MarkdownVisitor.Process(node).Trim('\n', '\r');

            // TODO: figure out of this is a problem
            content = content.Replace("&lt;", "<").Replace("&gt;", ">");

            var useBlockFormat = content.IndexOf('\n') > 0;

            if (useBlockFormat)
            {
                // TODO: detect language
                var attClass = node.Attributes.GetValueOrDefault("class", "");
                var language = string.Empty;
                foreach(var att in attClass.Split(' '))
                {
                    if(att.StartsWith("lang:"))
                    {
                        language = att.Replace("lang:", "");
                        break;
                    }
                }

                bodyBuilder.AppendLine().Append("```");
                bodyBuilder.AppendLine(language);
                bodyBuilder.AppendLine(content);
                bodyBuilder.AppendLine("```"); 
            } else
            {
                bodyBuilder.Append("`").Append(content).Append("`");
            }

            return false;
        }
    }
}
