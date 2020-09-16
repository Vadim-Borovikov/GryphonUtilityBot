using System.Collections.Generic;
using Newtonsoft.Json;
using ShoppingHelper.Logic;

namespace ShoppingHelper.Bot.Console
{
    internal sealed class Configuration
    {
        [JsonProperty]
        public string Token { get; set; }
        [JsonProperty]
        public int MasterId { get; set; }
        [JsonProperty]
        public List<Item> Items { get; set; }
    }
}
