using System.Collections.Generic;
using Newtonsoft.Json;

namespace GryphonUtility.Bot.Web.Models.Save
{
    internal sealed class Data
    {
        [JsonProperty]
        public int LastMassageId { get; set; }
        [JsonProperty]
        public Dictionary<int, string> Messages { get; set; } = new Dictionary<int, string>();
    }
}