using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GryphonUtility.Bot.Web.Models.Save
{
    public sealed class Record
    {
        [JsonProperty]
        public int MessageId { get; set; }

        [JsonProperty]
        public long ChatId { get; set; }

        [JsonProperty]
        public DateTime DateTime { get; set; }

        [JsonProperty]
        public List<string> Tags { get; set; } = new List<string>();
    }
}
