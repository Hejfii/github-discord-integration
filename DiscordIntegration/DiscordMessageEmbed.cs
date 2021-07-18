using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DiscordIntegration
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DiscordMessageEmbed
    {
        /// <summary>
        /// Color code of the embed. Has to be Decimal
        /// </summary>
        [JsonProperty("color")]
        public int? Color { get; set; }
        
        /// <summary>
        /// Embed author object
        /// </summary>
        [JsonProperty("author")]
        public DiscordMessageEmbedAuthor Author { get; set; }
        
        /// <summary>
        /// Title of embed
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }
        
        /// <summary>
        /// Url of embed. If title was used, it becomes hyperlink
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Description text
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Array of embed filed objects
        /// </summary>
        [JsonProperty("fields")]
        public DiscordMessageEmbedField[] Fields { get; set; }
        
        /// <summary>
        /// Embed thumbnail object
        /// </summary>
        [JsonProperty("thumbnail")]
        public DiscordMessageEmbedThumbnail Thumbnail { get; set; }
        
        /// <summary>
        /// Embed image object
        /// </summary>
        [JsonProperty("image")]
        public DiscordMessageEmbedImage Image { get; set; }
        
        /// <summary>
        /// Embed footer object
        /// </summary>
        [JsonProperty("footer")]
        public DiscordMessageEmbedFooter Footer { get; set; }

        [JsonConstructor]
        private DiscordMessageEmbed()
        {
            
        }

        public DiscordMessageEmbed(
            string title = null,
            string color = null,
            DiscordMessageEmbedAuthor author = null,
            string url = null,
            string description = null,
            IEnumerable<DiscordMessageEmbedField> fields = null,
            DiscordMessageEmbedThumbnail thumbnail = null,
            DiscordMessageEmbedImage image = null,
            DiscordMessageEmbedFooter footer = null)
        {
            this.Color = color != null ? int.Parse(color, System.Globalization.NumberStyles.HexNumber) : null;
            this.Author = author;
            this.Title = title?.Trim();
            this.Url = url?.Trim();
            this.Description = description?.Trim();
            this.Fields = fields?.ToArray();
            this.Thumbnail = thumbnail;
            this.Image = image;
            this.Footer = footer;
        }
    }

    /// <summary>
    /// Embed author object
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DiscordMessageEmbedAuthor
    {
        /// <summary>
        /// Name of author
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// Url of author. If name was used, it becomes a hyperlink
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }
        
        /// <summary>
        /// Url of author icon
        /// </summary>
        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonConstructor]
        private DiscordMessageEmbedAuthor()
        {
            
        }

        public DiscordMessageEmbedAuthor(string name = null, string url = null, string iconUrl = null)
        {
            this.Name = name?.Trim();
            this.Url = url?.Trim();
            this.IconUrl = iconUrl?.Trim();
        }
    }

    /// <summary>
    /// Embed field objects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DiscordMessageEmbedField
    {
        /// <summary>
        /// The name of the field
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// The value of the field
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
        
        /// <summary>
        /// If true, fields will be displayed in the same line, but there can only be 3 max in the same line or 2 max if you used thumbnail
        /// </summary>
        [JsonProperty("inline")]
        public bool InLine { get; set; }

        [JsonConstructor]
        private DiscordMessageEmbedField()
        {
            
        }

        public DiscordMessageEmbedField(string name, string value = null, bool inLine = false)
        {
            Name = name;
            Value = value;
            InLine = inLine;
        }
    }

    /// <summary>
    /// Embed thumbnail objects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DiscordMessageEmbedThumbnail
    {
        /// <summary>
        /// Url of thumbnail
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonConstructor]
        private DiscordMessageEmbedThumbnail()
        {
            
        }

        public DiscordMessageEmbedThumbnail(string url)
        {
            this.Url = url?.Trim();
        }
    }

    /// <summary>
    /// Embed image objects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DiscordMessageEmbedImage
    {
        /// <summary>
        /// Url of image
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonConstructor]
        private DiscordMessageEmbedImage()
        {
            
        }

        public DiscordMessageEmbedImage(string url)
        {
            this.Url = url?.Trim();
        }
    }

    /// <summary>
    /// Embed footer objects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class DiscordMessageEmbedFooter
    {
        /// <summary>
        /// Footer Text, doesn't support Markdown
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }
        
        /// <summary>
        /// Url of footer icon
        /// </summary>
        [JsonProperty("icon_url")]
        public string? IconUrl { get; set; }

        [JsonConstructor]
        private DiscordMessageEmbedFooter()
        {
            
        }

        public DiscordMessageEmbedFooter(string text, string? iconUrl = null)
        {
            this.Text = text?.Trim();
            this.IconUrl = iconUrl?.Trim();
        }
    }
}