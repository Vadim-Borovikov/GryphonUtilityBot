using System.Collections.Generic;
using AbstractBot;
using GryphonUtilityBot.Shop;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Bot
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

        [JsonProperty]
        public string GoogleSheetIdVinland { get; set; }

        [JsonProperty]
        public string GoogleVinlandCharactersRange { get; set; }

        [JsonProperty]
        public string GoogleVinlandConnectionsRange { get; set; }

        [JsonProperty]
        public string GoogleVinlandActivitiesRange { get; set; }
    }
}
