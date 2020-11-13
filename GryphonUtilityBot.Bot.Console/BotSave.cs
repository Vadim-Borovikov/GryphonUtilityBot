using System.Collections.Generic;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Bot.Console
{
    internal sealed class BotSave
    {
        [JsonProperty]
        public int LastMassageId { get; set; }
        [JsonProperty]
        public Dictionary<int, string> Messages { get; set; } = new Dictionary<int, string>();
    }
}