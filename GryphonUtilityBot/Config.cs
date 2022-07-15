using System.Collections.Generic;
using AbstractBot;
using GryphonUtilityBot.Shop;
using Newtonsoft.Json;

namespace GryphonUtilityBot
{
    public class Config : ConfigGoogleSheets
    {
        [JsonProperty]
        public string GoogleCredentialJson { get; set; }

        [JsonProperty]
        public int MistressId { get; set; }

        [JsonProperty]
        public List<Item> Items { get; set; }

        [JsonProperty]
        public string SavePath { get; set; }

        [JsonProperty]
        public string GoogleRange { get; set; }
    }
}
