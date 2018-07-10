using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SlothCord.Objects
{
    public struct DiscordMessageActivity
    {
        [JsonProperty("type")]
        public ActivityType Type { get; set; }

        [JsonProperty("party_id")]
        public string Party { get; private set; }
    }

    public struct DiscordActivity
    {
        [JsonProperty("url")]
        public string Url { get; private set; }

        [JsonProperty("type")]
        public ActivityType Type { get; set; }

        [JsonProperty("party_id")]
        public ulong? PartyId { get; private set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("application_id")]
        public long? ApplicationId { get; private set; }

        [JsonProperty("state")]
        public string State { get; private set; }

        [JsonProperty("details")]
        public string Details { get; private set; }

        [JsonProperty("timestamps")]
        public ActivityTimestamps Timestamps { get; private set; }

        [JsonProperty("party")]
        public ActivityParty Party { get; private set; }

        [JsonProperty("assets")]
        public ActivityAssets Assets { get; private set; }

        [JsonProperty("secrets")]
        public ActivitySecrets Secrets { get; private set; }

        [JsonProperty("instance")]
        public bool? InGame { get; private set; }

        [JsonProperty("flags")]
        public int? Flags { get; private set; }
    }

    public struct ActivitySecrets
    {
        [JsonProperty("join")]
        public string Join { get; private set; }

        [JsonProperty("spectate")]
        public string Spectate { get; private set; }

        [JsonProperty("match")]
        public string Match { get; private set; }
    }

    public struct ActivityAssets
    {
        [JsonProperty("large_image")]
        public string LargeImage { get; private set; }

        [JsonProperty("large_text")]
        public string LargeText { get; private set; }

        [JsonProperty("small_image")]
        public string SmallImage { get; private set; }

        [JsonProperty("small_text")]
        public string SmallText { get; private set; }
    }

    public struct ActivityParty
    {
        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("size")]
        public IEnumerable<int> Size { get; private set; }
    }

    public struct ActivityTimestamps
    {
        [JsonProperty("start")]
        public ulong? Start { get; private set; }

        [JsonProperty("end")]
        public ulong? End { get; private set; }
    }
}