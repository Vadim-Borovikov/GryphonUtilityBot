using System.Collections.Generic;
using AbstractBot;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Bot.Config
{
    public class Config : ConfigGoogleSheets
    {
        [JsonProperty]
        public int MistressId { get; set; }

        [JsonProperty]
        public List<ShopItem> Items { get; set; }

        [JsonProperty]
        public string SavePath { get; set; }

        [JsonProperty]
        public string GoogleRange { get; set; }
    }
}
