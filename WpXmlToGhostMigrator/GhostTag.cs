using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace WpXmlToGhostMigrator
{
    public class GhostTag : ITransformableElement
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonIgnore]
        public string ParentName { get; set; }

        [JsonIgnore]
        public List<GhostTag> Children { get; } = new List<GhostTag>();

        public void Validate()
        {
            if (string.IsNullOrEmpty(Slug))
            {
                this.Slug = (Name ?? Guid.NewGuid().ToString()).ToSlug();
            }
        }

        [JsonIgnore]
        public List<GhostPost> PostsUsingTag { get; } = new List<GhostPost>();

        public void AddLinkToPost(GhostPost post)
        {
            this.PostsUsingTag.Add(post);
        }
    }

    public static class GhostTagVisitor
    {
        public static void Visit(XElement node, GhostTag item, Dictionary<string, GhostTag> tags)
        {
            switch (node.Name.LocalName.ToLower())
            {
                case "term_id":
                    item.Id = Int32.Parse(node.Value);
                    return;
                case "category_nicename":
                case "tag_slug":
                    item.Slug = node.Value;
                    return;
                case "cat_name":
                case "tag_name":
                    item.Name = node.Value;
                    return;
                case "category_parent":
                    item.ParentName = node.Value;
                    if (tags.TryGetValue(node.Value, out GhostTag parent))
                    {
                        parent.Children.Add(item);
                    }
                    return;
                default:
                    break;
            }
        }
    }
}
