using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WpXmlToGhostMigrator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {

                // Console.WriteLine("Enter a file location to open:");
                // args = new string[] { Console.ReadLine() };
                args = new string[] { "/Users/anze/Dev/blog.xml" };
            }

            foreach (var file in args)
            {
                ProcessFile(file);
            }

            Console.ReadLine();
        }

        private static void ProcessFile(string path)
        {
            // guard the path, that it exists
            if (!System.IO.File.Exists(path))
            {
                WriteOutput($"File {path} does not exist, or you do not have permissions to write to.", LoggingLevels.Error);
            }

            Console.WriteLine($"Processing file on {path}");

            // load the document
            XDocument doc;
            try
            {
                doc = XDocument.Load(path);

            }
            catch (Exception e)
            {
                WriteOutput(e.Message, LoggingLevels.Error);
                return;
            }

            var channelNode = doc.Descendants().SingleOrDefault(x => x.Name.LocalName == "channel");
            if (channelNode == null)
            {
                WriteOutput("The file is missing a channel node.", LoggingLevels.Error);
            }

            // build categories and tags tree
            // and this needs to be special, as some tags are self-referencing... of course

            var elements = channelNode.Elements()
                .Where(x => x.Name.LocalName == "category").ToArray();
            var categories = new Dictionary<string, GhostTag>();
            foreach (var element in elements)
            {
                var tag = Process<GhostTag>(element, (node, item) => GhostTagVisitor.Visit(node, item, categories));
                categories.Add(tag.Slug, tag);
            }


            elements = channelNode.Elements()
                .Where(x => x.Name.LocalName == "tag").ToArray();
            var tags = new Dictionary<string, GhostTag>();
            foreach (var element in elements)
            {
                var tag = Process<GhostTag>(element, (node, item) => GhostTagVisitor.Visit(node, item, tags));
                tags.Add(tag.Slug, tag);
            }

            var attachments = channelNode.Elements()
              .Where(x => x.Name.LocalName == "item" && x.Elements().Single(p => p.Name.LocalName == "post_type").Value == "attachment")
              .Select(p => Process<WordpressAttachment>(p, (node, item) => WordpressAttachmentVisitor.Visit(node, item)))
              .ToDictionary(x => x.Id, x => x);

            var posts = channelNode.Elements()
                .Where(x => x.Name.LocalName == "item" && x.Descendants().Single(p => p.Name.LocalName == "post_type").Value == "post")
                .Select(p => Process<GhostPost>(p, (node, item) => GhostPostVisitor.Process(node, item)))
                .ToDictionary(x => x.Id, x => x);

            // build the final post
            foreach (var post in posts.Values)
            {
                post.BuildLinks(attachments, categories, tags);

                // transform the image url
                post.Image = TransformUrl(post.Image);
            }

            // build the document
            var data = new GhostExportDocumentData
            {
                Posts = posts.Values.ToList(),
                Categories = categories.Values.Where(x => x.PostsUsingTag.Count > 0).ToList()
            };

            var document = new GhostExportDocument(data, new GhostExportDocumentMetadata());

            Console.WriteLine(JsonConvert.SerializeObject(document));
        }

        private static string TransformUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return url;

            // rules come here
            return url;
        }

        private const string NS_Content = "http://purl.org/rss/1.0/modules/content/";
        private const string NS_WP = "http://wordpress.org/export/1.2/";

        private static T Process<T>(XElement element, Action<XElement, T> call, Func<XElement, bool> filterFunc = null) where T : ITransformableElement, new()
        {
            var result = new T();

            var elements = element.Elements();
            if (filterFunc != null)
            {
                elements = elements.Where(filterFunc);
            }

            foreach (var node in elements)
            {
                call(node, result);
            }

            result.Validate();

            return result;
        }


        private static void WriteOutput(string message, LoggingLevels level = LoggingLevels.Trace)
        {
            switch (level)
            {
                case LoggingLevels.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    Console.ResetColor();
                    break;
            }

            Console.WriteLine(message);
            Console.ResetColor();
        }

        private enum LoggingLevels
        {
            Debug,
            Trace,
            Error
        }
    }
}
