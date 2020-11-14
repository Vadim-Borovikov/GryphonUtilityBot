using System.Collections.Generic;
using Newtonsoft.Json;

namespace GryphonUtility.Bot.Web.Models.Save
{
    internal sealed class Data
    {
        [JsonProperty]
        public List<Article> Articles { get; set; } = new List<Article>();
        [JsonProperty]
        public SortedDictionary<int, string> Messages { get; set; } = new SortedDictionary<int, string>();
    }
}