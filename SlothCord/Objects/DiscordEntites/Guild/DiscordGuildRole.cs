using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlothCord.Objects
{
    public struct DiscordGuildRole
    {
        [JsonProperty("position")]
        public int Postition { get; private set; }

        [JsonProperty("permissions")]
        public Permissions Permissions { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("Mentionable")]
        public bool Mentionable { get; private set; }

        [JsonProperty("managed")]
        public bool Managed { get; private set; }

        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("hoist")]
        public bool IsHoisted { get; private set; }

        [JsonProperty("color")]
        private int IntColorValue { get; set; }

        [JsonIgnore]
        public string Mention { get => $"<@&{this.Id}>"; }
    }
}
