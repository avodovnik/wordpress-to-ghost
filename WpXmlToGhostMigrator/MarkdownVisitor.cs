using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace WpXmlToGhostMigrator
{
    public static class MarkdownVisitor
    {
        public static string Process(HtmlParser.HtmlDocument document)
        {
            var bodyBuilder = new StringBuilder();
            foreach (var node in document.Children)
            {
                ProcessNode(node, bodyBuilder);
            }
            return bodyBuilder.ToString();
        }

        private static void ProcessNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            if (node is HtmlParser.TextNode)
            {
                bodyBuilder.AppendLine(node.ToString());
            }

            foreach(var child in node.Children) {
                ProcessNode(child, bodyBuilder);
            }
        }
    }
}