using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WpXmlToGhostMigrator
{
    public static class GhostPostVisitor
    {
        public static GhostPost Process(XElement node, GhostPost post)
        {
            switch (String.Join(':', node.Name.NamespaceName, node.Name.LocalName).ToLower().TrimStart(':'))
            {
                case "title":
                    post.Title = node.Value;
                    // Console.WriteLine($"Parsing {node.Value}");
                    break;
                case "link":
                    // we'll only use this to later check the post
                    post.OldUrl = node.Value;
                    break;
                case "pubdate":
                    post.Published = ReadDate(node.Value);
                    break;
                case "http://wordpress.org/export/1.2/:post_date_gmt":
                    post.Created = ReadDate(node.Value);
                    break;
                case "http://purl.org/dc/elements/1.1/:creator":
                    // Console.WriteLine("Creator: " + node.Value);
                    // TODO: implement
                    break;
                case "description":
                    post.MetaDescription = node.Value;
                    break;
                case "http://wordpress.org/export/1.2/:postmeta":
                    ReadMeta(node, post);
                    break;
                case "http://wordpress.org/export/1.2/:post_name":
                    post.Slug = node.Value;
                    break;
                case "http://wordpress.org/export/1.2/:status":
                    post.Status = ReadStatus(node.Value);
                    break;
                case "http://wordpress.org/export/1.2/:post_id":
                    post.Id = ReadNumber(node.Value).GetValueOrDefault(-1);
                    break;
                case "category":
                    ReadCategory(node, post);
                    break;
                case "http://purl.org/rss/1.0/modules/content/:encoded":
                    ReadContent(node, post);
                    break;

                case "http://wordpress.org/export/1.2/:post_date":
                    // ignore
                    break;

                default:
                    // Console.WriteLine("Panic!");
                    // Console.WriteLine($"\t\t{node.Name.NamespaceName}::{node.Name.LocalName}");
                    // Console.WriteLine($"\t\t\t{node.Value}");
                    break;
            }
            //Console.WriteLine($"\t\t{node.Name.NamespaceName}::{node.Name.LocalName}");

            return post;
        }

        private static void ReadContent(XElement node, GhostPost post)
        {
            // start parsing the content
            var htmlContent = PreprocessContent(node.Value);

            var html = HtmlParser.Parse(htmlContent);
            html.Name = post.Title;

            var content = MarkdownVisitor.Process(html);
            post.Markdown = content;
        }

        private static string PreprocessContent(string content)
        {
            // TODO: move this to something else
            Regex gallery = new Regex(@"(\[)(gallery.*?)(\])", RegexOptions.None);
            Regex caption = new Regex(@"(\[)(\/?caption.*?)(\])", RegexOptions.None);

            // there are some "pre-processing" things we need to do, and this 
            // is the perfect place to do it
            content = gallery.Replace(content, m => $"<wp-{m.Groups[2].Value}/>");

            // out caption match is slightly different, in that we expect a closing tag
            content = caption.Replace(content, m => $"<{m.Groups[2].Value}>"); 
            return content;
        }

        private static void ReadCategory(XElement node, GhostPost post)
        {
            var domain = node.Attributes().Single(x => x.Name == "domain");
            var nicename = node.Attributes().Single(x => x.Name == "nicename");

            switch (domain.Value)
            {
                case "post_tag":
                    post.Tags.Add(nicename.Value);
                    break;

                case "category":
                    post.Categories.Add(nicename.Value);
                    break;

                case "post_format":
                    break;

                default:
                    throw new Exception("Panic.");
            }
        }

        private static int? ReadNumber(string value)
        {
            if (!int.TryParse(value, out int result))
                return null;
            return result;
        }
        private static Status ReadStatus(string value)
        {
            if (!Enum.TryParse<Status>(value, true, out Status status))
            {
                throw new Exception("Status not found " + value);
            }

            return status;
        }
        private static DateTime ReadDate(string value)
        {
            DateTime dt;
            if (!DateTime.TryParse(value, CultureInfo.InvariantCulture.DateTimeFormat, System.Globalization.DateTimeStyles.AssumeUniversal, out dt))
            {
                if (!DateTime.TryParse(value, out dt))
                {
                    return DateTime.Now;
                }
            }
            return dt.ToUniversalTime();
        }

        private static void ReadMeta(XElement node, GhostPost post)
        {
            // the node should have two elements
            var elements = node.Elements();
            if (elements.Count() != 2)
            {
                // TODO: Panic
                return;
            }

            // the key
            var key = elements.Single(x => x.Name.LocalName == "meta_key");

            // and the value
            var value = elements.Single(x => x.Name.LocalName == "meta_value");

            post.Meta.Add(key.Value, value.Value);
        }

        private static void ParseHtmlToMarkdown(string html)
        {

        }
    }
}


/*
 * 
 * A look at Computer Vision
                ::title
                ::link
                ::pubDate
                http://purl.org/dc/elements/1.1/::creator
                ::guid
                ::description
                http://purl.org/rss/1.0/modules/content/::encoded
                http://wordpress.org/export/1.2/excerpt/::encoded
                http://wordpress.org/export/1.2/::post_id

                http://wordpress.org/export/1.2/::comment_status
                http://wordpress.org/export/1.2/::ping_status
                
                http://wordpress.org/export/1.2/::status
                http://wordpress.org/export/1.2/::post_parent
                http://wordpress.org/export/1.2/::menu_order
                http://wordpress.org/export/1.2/::post_type
                http://wordpress.org/export/1.2/::post_password
                http://wordpress.org/export/1.2/::is_sticky
                ::category



                http://wordpress.org/export/1.2/::postmeta
                http://wordpress.org/export/1.2/::meta_key
                http://wordpress.org/export/1.2/::meta_value
                http://wordpress.org/export/1.2/::postmeta
                http://wordpress.org/export/1.2/::meta_key
                http://wordpress.org/export/1.2/::meta_value
                http://wordpress.org/export/1.2/::postmeta
                http://wordpress.org/export/1.2/::meta_key
                http://wordpress.org/export/1.2/::meta_value
                http://wordpress.org/export/1.2/::postmeta
                http://wordpress.org/export/1.2/::meta_key
                http://wordpress.org/export/1.2/::meta_value
                http://wordpress.org/export/1.2/::postmeta
                http://wordpress.org/export/1.2/::meta_key
                http://wordpress.org/export/1.2/::meta_value
                http://wordpress.org/export/1.2/::postmeta
                http://wordpress.org/export/1.2/::meta_key
                http://wordpress.org/export/1.2/::meta_value
                http://wordpress.org/export/1.2/::postmeta
                http://wordpress.org/export/1.2/::meta_key
                http://wordpress.org/export/1.2/::meta_value
                http://wordpress.org/export/1.2/::postmeta
                http://wordpress.org/export/1.2/::meta_key
                http://wordpress.org/export/1.2/::meta_value

    */
