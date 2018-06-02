using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlothCord.Objects
{
    public struct DiscordGuildEmoji
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("roles")]
        public IReadOnlyList<DiscordGuildRole> Roles { get; private set; }

        [JsonProperty("user")]
        public DiscordUser Creator { get; private set; }

        [JsonProperty("requires_colons")]
        public bool? RequiresColons { get; private set; }

        [JsonProperty("managed")]
        public bool IsManaged { get; private set; }

        [JsonProperty("animated")]
        public bool IsAnimated { get; private set; }
    }
}
