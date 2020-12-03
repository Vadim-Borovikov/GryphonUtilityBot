﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace GryphonUtility.Bot.Web.Models.Config
{
    public sealed class Config
    {
        [JsonProperty]
        public string Token { get; set; }
        [JsonProperty]
        public int MasterId { get; set; }
        [JsonProperty]
        public int MistressId { get; set; }
        [JsonProperty]
        public List<Item> Items { get; set; }
        [JsonProperty]
        public string SavePath { get; set; }
        [JsonProperty]
        public string Host { get; set; }
        [JsonProperty]
        public int Port { get; set; }
        [JsonProperty]
        public string CultureInfoName { get; set; }

        internal string Url => $"{Host}:{Port}/{Token}";
    }
}
