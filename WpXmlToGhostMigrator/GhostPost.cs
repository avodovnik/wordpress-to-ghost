using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace WpXmlToGhostMigrator
{
    public class GhostPost : ITransformableElement
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("featured")]
        public int Featured { get; set; } = 0;

        [JsonProperty("status")]
        [JsonConverter(typeof(StatusConverter))]
        public Status Status { get; set; }

        [JsonProperty("meta_title")]
        public string MetaTitle { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("meta_description")]
        public string MetaDescription { get; set; }

        [JsonProperty("created_at")]
        public DateTime Created { get; set; }

        [JsonProperty("updated_at")]
        public DateTime? Updated { get; set; }

        [JsonProperty("published_at")]
        public DateTime? Published { get; set; }

        [JsonProperty("author_id")]
        public Author Author { get; set; }

        [JsonProperty("created_by")]
        public Author CreatedBy { get; set; }

        [JsonProperty("updated_by")]
        public Author UpdatedBy { get; set; }

        [JsonProperty("published_by")]
        public Author PublishedBy { get; set; }

        [JsonProperty("page")]
        public bool IsPage { get; set; }

        [JsonProperty("html")]
        public string Html { get; } = null; // we don't support HTML, as we want to convert

        [JsonProperty("markdown")]
        public string Markdown { get; set; } = "No content, yet."; 

        /// <summary>
        /// This property is used to run a check at the end.
        /// </summary>
        [JsonIgnore()]
        public string OldUrl { get; set; }

        // TODO: for now
        [JsonIgnore]
        public IDictionary<string, string> Meta { get; private set; } = new Dictionary<string, string>();

        [JsonIgnore]
        public List<string> Categories { get; } = new List<string>();

        [JsonIgnore]
        public List<string> Tags { get; } = new List<string>();

        /// <summary>
        /// Called when all the elements have been processed. This applied additional rules, like
        /// ensures that the slug is there, etc.
        /// </summary>
        public void Validate()
        {
            // slug should not be empty
            if (string.IsNullOrEmpty(this.Slug))
            {
                this.Slug = this.Title.ToSlug();
            }

            // if status is private, add [Private] to the title
            if(this.Status == Status.Private)
            {
                this.Title = $"[Private] {Title}";
            }

            // update the dates, starting from the published one
            if(this.Created == null)
            {
                Created = Published ?? DateTime.Now.AddYears(-1);
            }
        }

        internal void BuildLinks(Dictionary<int, WordpressAttachment> attachments, 
            Dictionary<string, GhostTag> categories, 
            Dictionary<string, GhostTag> tags)
        {
            // we find _thumbnail_id and map to the attachments list, hopefully
            if (this.Meta.TryGetValue("_thumbnail_id", out string value))
            {
                if (Int32.TryParse(value, out int id))
                {
                    if (attachments.TryGetValue(id, out WordpressAttachment attachment))
                    {
                        this.Image = attachment.AttachmentUrl;
                    } else
                    {
                        System.Diagnostics.Debug.WriteLine("Could not parse attachment correctly.");
                    }
                }
            }

            foreach(var category in this.Categories)
            {
                if(categories.TryGetValue(category, out GhostTag tag))
                {
                    tag.AddLinkToPost(this);
                }
            }

            foreach(var tag in this.Tags)
            {
                if(tags.TryGetValue(tag, out GhostTag xTag))
                {
                    xTag.AddLinkToPost(this);
                }
            }

            System.Diagnostics.Debug.WriteLine($"Amount of categories: {this.Categories.Count} and amount of tags: {this.Tags.Count}");
        }
    }

    public class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return this.Id.ToString();
        }
    }

    public enum Status
    {
        Publish,
        Draft,
        Private //not supported by Ghost, of course
    }

    public class StatusConverter : JsonConverter<Status>
    {
        public override Status ReadJson(JsonReader reader, Type objectType, Status existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return Enum.Parse<Status>(reader.ReadAsString()); 
        }

        public override void WriteJson(JsonWriter writer, Status value, JsonSerializer serializer)
        {
            switch (value)
            {
                case Status.Publish:
                    writer.WriteValue("published");
                    break;
                case Status.Draft:
                case Status.Private:
                    writer.WriteValue("draft");
                    break;
            }
        }
    }
}
