using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlothCord.Objects
{
    public struct DiscordAttachment
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("filename")]
        public string FileName { get; private set; }

        [JsonProperty("size")]
        public int Size { get; private set; }

        [JsonProperty("url")]
        public string Url { get; private set; }

        [JsonProperty("proxy_url")]
        public string ProxyUrl { get; private set; }

        [JsonProperty("height")]
        public int Height { get; private set; }

        [JsonProperty("width")]
        public int Width { get; private set; }
    }
}