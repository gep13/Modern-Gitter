﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Gitter.Model
{
    public class Room
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("oneToOne")]
        public bool OneToOne { get; set; }

        [JsonProperty("users")]
        public IList<User> Users { get; set; }

        [JsonProperty("userCount")]
        public int UserCount { get; set; }

        [JsonProperty("unreadItems")]
        public int UnreadItems { get; set; }

        [JsonProperty("mentions")]
        public int UnreadMentions { get; set; }

        [JsonProperty("lastAccessTime")]
        public DateTime LastAccessTime { get; set; }

        [JsonProperty("lurk")]
        public bool DisabledNotifications { get; set; }

        [JsonProperty("githubType")]
        public string Type { get; set; }

        [JsonProperty("v")]
        public int Version { get; set; }
    }
}
