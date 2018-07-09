using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static WpXmlToGhostMigrator.Program;

namespace WpXmlToGhostMigrator.Converters
{
    public class GalleryConverter : MarkdownConverterBase
    {
        public override string[] SupportedTags => new string[] { "wp-gallery" };

        public override void CloseNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
        }

        public override bool OpenNode(HtmlParser.Node node, StringBuilder bodyBuilder)
        {
            if (!node.Attributes.TryGetValue("ids", out string keys))
            {
                throw new Exception("Gallery had no ids, no idea what to do here.");
            }

            var ids = keys.Split(',').Select(x => Int32.Parse(x)).ToArray();

            // Ugh... :-(
            foreach (var id in ids)
            {
                var attachment = FileContext.Current.Attachments.SingleOrDefault(x => x.Key == id).Value;
                if (attachment == null)
                {
                    // todo: what do to here?
                    continue;
                };

                // not sure I like this, but again, let's do what we have to
                var imageNode = new HtmlParser.Node()
                {
                    Name = "img",
                    Type = HtmlParser.Node.NodeType.SelfClosing,
                    Attributes = new Dictionary<string, string>() { { "src", attachment.AttachmentUrl } }
                };

                // the gallery approach would have likely been better if I decided to play with the 
                // columns, and then split into lines, etc. But this is a good enough start.
                //bodyBuilder.AppendLine(MarkdownVisitor.Process(imageNode));

                node.AddChild(imageNode);
            }

            return true;
        }
    }
}
