using System.Collections.Generic;
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
        public string RecordsPath { get; set; }

        [JsonProperty]
        public string Host { get; set; }

        [JsonProperty]
        public int Port { get; set; }

        [JsonProperty]
        public string CultureInfoName { get; set; }

        [JsonProperty]
        public string SystemTimeZoneId { get; set; }

        [JsonProperty]
        public string DontUnderstandStickerFileId { get; set; }

        [JsonProperty]
        public string ForbiddenStickerFileId { get; set; }

        [JsonProperty]
        public Dictionary<string, string> GoogleCredentials { get; set; }

        [JsonProperty]
        public string GoogleCredentialsJson { get; set; }

        [JsonProperty]
        public string GoogleSheetId { get; set; }

        [JsonProperty]
        public string GoogleRange { get; set; }

        internal string Url => $"{Host}:{Port}/{Token}";
    }
}
