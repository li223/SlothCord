using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlothCord.Objects
{
    public sealed class DiscordEmbed
    {
        public DiscordEmbed AddField(EmbedField field)
        {
            this.PrivateEmbedFields.Add(field);
            return this;
        }

        public DiscordEmbed AddField(string name, string value, bool inline = false)
        {
            this.PrivateEmbedFields.Add(new EmbedField()
            {
                IsInline = inline,
                Name = name,
                Value = value
            });
            return this;
        }

        public DiscordEmbed AddAuthor(EmbedAuthor author)
        {
            this.Author = author;
            return this;
        }

        public DiscordEmbed AddAuthor(string name, string url = null, string icon_url = null, string proxy_icon_url = null)
        {
            this.Author = new EmbedAuthor()
            {
                Name = name,
                Url = url,
                IconUrl = icon_url,
                ProxyIconUrl = proxy_icon_url
            };
            return this;
        }

        public DiscordEmbed AddFooter(EmbedFooter footer)
        {
            this.Footer = footer;
            return this;
        }

        public DiscordEmbed AddFooter(string text, string icon_url = null, string proxy_icon_url = null)
        {
            var footer = new EmbedFooter()
            {
                Text = text,
                IconUrl = icon_url,
                ProxyIconUrl = proxy_icon_url
            };
            this.Footer = footer;
            return this;
        }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public string Type { get; private set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty("color", NullValueHandling = NullValueHandling.Ignore)]
        public int IntColor { get; set; }

#if NETCORE
        [JsonIgnore]
        public DiscordColor Color
        {
            get
            {
                Enum.TryParse(typeof(DiscordColor), this.IntColor.ToString(), out object res);
                return (DiscordColor)res;
            }
            set { this.IntColor = (int)value; }
        }
#else
        [JsonIgnore]
        public DiscordColor Color
        {
            get
            {
                Enum.TryParse(this.IntColor.ToString(), out DiscordColor res);
                return res;
            }
            set { this.IntColor = (int)value; }
        }
#endif

        [JsonProperty("footer", NullValueHandling = NullValueHandling.Ignore)]
        public EmbedFooter Footer { get; set; }

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public EmbedImage Image { get; set; }

        [JsonProperty("thumbnail", NullValueHandling = NullValueHandling.Ignore)]
        public EmbedThumbnail Thumbnail { get; set; }

        [JsonProperty("video", NullValueHandling = NullValueHandling.Ignore)]
        public EmbedVideo Video { get; private set; }

        [JsonProperty("provider", NullValueHandling = NullValueHandling.Ignore)]
        public EmbedProvider Provider { get; private set; }

        [JsonProperty("author", NullValueHandling = NullValueHandling.Ignore)]
        public EmbedAuthor Author { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public IReadOnlyList<EmbedField> Fields { get => this.PrivateEmbedFields; private set { this.Fields = PrivateEmbedFields; } }

        [JsonIgnore]
        private List<EmbedField> PrivateEmbedFields = new List<EmbedField>();
    }

    public struct EmbedFooter
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; set; }
    }

    public struct EmbedImage
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; set; }

        [JsonProperty("height")]
        public int Height { get; private set; }

        [JsonProperty("width")]
        public int Width { get; private set; }
    }

    public struct EmbedThumbnail
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; set; }

        [JsonProperty("height")]
        public int Height { get; private set; }

        [JsonProperty("width")]
        public int Width { get; private set; }
    }

    public struct EmbedVideo
    {
        [JsonProperty("url")]
        public string Url { get; private set; }

        [JsonProperty("height")]
        public int Height { get; private set; }

        [JsonProperty("width")]
        public int Width { get; private set; }
    }

    public struct EmbedProvider
    {
        [JsonProperty("url")]
        public string Url { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }
    }

    public struct EmbedAuthor
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("proxy_icon_url")]
        public string ProxyIconUrl { get; set; }

        [JsonProperty("icon_url")]
        public string IconUrl { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public sealed class EmbedField
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("inline")]
        public bool IsInline { get; set; }
    }

    public struct GuildEmbed
    {
        [JsonProperty("enabled")]
        public bool IsEnabled { get; internal set; }

        [JsonProperty("channel_id")]
        public ulong ChannelId { get; internal set; }
    }
}