using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpXmlToGhostMigrator
{
    public class GhostExportDocument
    {
        public GhostExportDocument(GhostExportDocumentData data, GhostExportDocumentMetadata metaData)
        {
            this.Data = data;
            this.Metadata = metaData;
        }

        [JsonProperty("data")]
        public GhostExportDocumentData Data { get; private set; }

        [JsonProperty("meta")]
        public GhostExportDocumentMetadata Metadata { get; private set; }
    }

    public class GhostExportDocumentMetadata
    {
        [JsonProperty("exported_on")]
        public DateTime ExportedOn { get; set; } = DateTime.UtcNow;

        [JsonProperty("version")]
        public string Version { get; } = "000";
    }

    public class GhostExportDocumentData
    {
        [JsonProperty("posts")]
        public List<GhostPost> Posts { get; set; }

        [JsonIgnore()]
        private List<GhostTag> _categories;

        [JsonProperty("tags")]
        public List<GhostTag> Categories
        {
            get
            {
                return _categories;
            }
            set
            {
                _categories = value;
                PostMappings = _categories.SelectMany(x => x.PostsUsingTag.Select(p => new GhostTagToPost() { PostId = p.Id, TagId = x.Id })).ToList();
            }
        }

        [JsonProperty("posts_tags")]
        public IEnumerable<GhostTagToPost> PostMappings { get; private set; }

        [JsonProperty("users")]
        public IEnumerable<object> Users { get; } = new object[0];

        public class GhostTagToPost
        {
            [JsonProperty("tag_id")]
            public int TagId { get; set; }

            [JsonProperty("post_id")]
            public int PostId { get; set; }
        }

    }
}
