using System;
using Newtonsoft.Json;
using Telegram.Bot.Types.Enums;

namespace GryphonUtility.Bot.Web.Models.Save
{
    public sealed class Record
    {
        [JsonProperty]
        public int MessageId { get; set; }

        [JsonProperty]
        public DateTime DateTime { get; set; }

        [JsonProperty]
        public MessageType Type { get; set; }

        [JsonProperty]
        public int AuthorId { get; set; }
    }
}
