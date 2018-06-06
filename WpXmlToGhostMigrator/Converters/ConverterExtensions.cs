using System;
using System.Collections.Generic;
using System.Text;
using static WpXmlToGhostMigrator.HtmlParser;

namespace WpXmlToGhostMigrator.Converters
{
    public static class ConverterExtensions
    {
        public static void RegisterConverter<T>(this Dictionary<string, Converters.MarkdownConverterBase> list) where T : Converters.MarkdownConverterBase, new()
        {
            var instance = new T();
            
            foreach(var tag in instance.SupportedTags)
            {
                list.Add(tag, instance);
            }
        }

        public static MarkdownConverterBase GetConverter(this Dictionary<string, Converters.MarkdownConverterBase> list, HtmlParser.Node node)
        {
            var tag = node.Name;

            if (!string.IsNullOrEmpty(tag) && list.ContainsKey(tag))
                return list[tag];

            return new DefaultConverter();
        }

        public static string GetNameSafely(this Node node)
        {
            return (node.Name ?? string.Empty).ToLower();
        }

    }
}
