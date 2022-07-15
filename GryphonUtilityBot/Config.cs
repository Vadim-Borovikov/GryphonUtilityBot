using System;
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
        public List<string> VinlandMorningPostfixLines { get; set; }

        [JsonProperty]
        public List<string> VinlandAfternoonPrefixLines { get; set; }

        [JsonProperty]
        public List<string> VinlandAfternoonPostfixLines { get; set; }

        [JsonProperty]
        public short VinlandActivityPriorityScore { get; set; }

        internal string VinlandMorningPrefixText =>
            VinlandMorningPrefixLines == null ? "" : string.Join(Environment.NewLine, VinlandMorningPrefixLines);

        internal string VinlandMorningPostfixText =>
            VinlandMorningPostfixLines == null ? "" : string.Join(Environment.NewLine, VinlandMorningPostfixLines);

        internal string VinlandAfternoonPrefixText =>
            VinlandAfternoonPrefixLines == null ? "" : string.Join(Environment.NewLine, VinlandAfternoonPrefixLines);

        internal string VinlandAfternoonPostfixText =>
            VinlandAfternoonPostfixLines == null ? "" : string.Join(Environment.NewLine, VinlandAfternoonPostfixLines);
    }
}
