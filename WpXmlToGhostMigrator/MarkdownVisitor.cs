using System;
using System.Collections.Generic;
using System.Text;
using WpXmlToGhostMigrator.Converters;

namespace WpXmlToGhostMigrator
{
    public static class MarkdownVisitor
    {
        private static Dictionary<string, Converters.MarkdownConverterBase> _converters = new Dictionary<string, Converters.MarkdownConverterBase>();

        static MarkdownVisitor()
        {
            _converters.RegisterConverter<BlockquoteConverter>();
            _converters.RegisterConverter<CodeConverter>();
            _converters.RegisterConverter<DivConverter>();
            _converters.RegisterConverter<EmphasisConverter>();
            _converters.RegisterConverter<HardBreakConverter>();
            _converters.RegisterConverter<HeaderConverter>();
            _converters.RegisterConverter<LinkConverter>();
            _converters.RegisterConverter<ImageConverter>();
            _converters.RegisterConverter<ListItemConverter>();
        }

        public static string Process(HtmlParser.Node document)
        {
            Console.WriteLine("Post ======= {0}", document.Name);

            var bodyBuilder = new StringBuilder();
            foreach (var node in document.Children)
            {
                ProcessNode(node, bodyBuilder);
            }
            return bodyBuilder.ToString();
        }

        private static void ProcessNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            var converter = _converters.GetConverter(node);

            if(converter.OpenNode(node, bodyBuilder))
            {
                // the converter returning true signifies that we are expecting
                // this node to have children and we should parse them
                foreach(var child in node.Children)
                {
                    ProcessNode(child, bodyBuilder);
                }
            }

            converter.CloseNode(node, bodyBuilder);
        }
    }
}