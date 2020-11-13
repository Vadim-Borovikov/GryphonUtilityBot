using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using GryphonUtilityBot.Logic;

namespace GryphonUtilityBot.Bot.Console
{
    internal sealed class Configuration
    {
        [JsonProperty]
        public string Token { get; set; }
        [JsonProperty]
        public int MasterId { get; set; }
        [JsonProperty]
        public List<Item> Items { get; set; }
        [JsonProperty]
        public List<Article> Articles { get; set; }
        [JsonProperty]
        public long ArticlesChannelChatId { get; set; }
        [JsonProperty]
        public int ArticlesFirstMessageId { get; set; }
        [JsonProperty]
        public string SavePath { get; set; }
        [JsonProperty]
        public int DelaySeconds { get; set; }

        public TimeSpan Delay => TimeSpan.FromSeconds(DelaySeconds);
    }
}
