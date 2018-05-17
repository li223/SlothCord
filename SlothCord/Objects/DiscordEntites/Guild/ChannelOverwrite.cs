using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlothCord.Objects
{
    public struct ChannelOverwrite
    {
        [JsonProperty("id")]
        public ulong Id { get; private set; }

        [JsonProperty("type")]
        public string Type { get; private set; }

        [JsonProperty("allow")]
        public int Allow { get; private set; }

        [JsonProperty("deny")]
        public int Deny { get; private set; }
    }
}
