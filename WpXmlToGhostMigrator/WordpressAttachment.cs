using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace WpXmlToGhostMigrator
{
    public class WordpressAttachment : ITransformableElement
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Link { get; set; }
        public string AttachmentUrl { get; set; }
        public int ParentId { get; set; }

        public void Validate()
        {
        }
    }

    public static class WordpressAttachmentVisitor
    {
        public static void Visit(XElement node, WordpressAttachment attachment)
        {
            switch(node.Name.LocalName.ToLower())
            {
                case "title":
                    attachment.Title = node.Value;
                    break;

                case "attachment_url":
                    attachment.AttachmentUrl = node.Value;
                    break;

                case "post_id":
                    attachment.Id = Int32.Parse(node.Value);
                    break;

                case "post_parent":
                    attachment.ParentId = Int32.Parse(node.Value);
                    break;

                default:
                    break;
            }
        }
    }
}
