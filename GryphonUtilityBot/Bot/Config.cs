using System;
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
        public string GoogleVinlandAbilitiesRange { get; set; }

        [JsonProperty]
        public string GoogleVinlandActivitiesRange { get; set; }

        [JsonProperty]
        public List<string> VinlandMorningPrefixLines { get; set; }

        [JsonProperty]
        public List<string> VinlandAfternoonPrefixLines { get; set; }

        [JsonProperty]
        public List<string> VinlandAfternoonPostfixLines { get; set; }

        internal string VinlandMorningPrefixText => string.Join(Environment.NewLine, VinlandMorningPrefixLines);
        internal string VinlandAfternoonPrefixText => string.Join(Environment.NewLine, VinlandAfternoonPrefixLines);
        internal string VinlandAfternoonPostfixText => string.Join(Environment.NewLine, VinlandAfternoonPostfixLines);
    }
}
