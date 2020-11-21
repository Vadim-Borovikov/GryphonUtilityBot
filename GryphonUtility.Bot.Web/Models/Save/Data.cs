using System.Collections.Generic;
using Newtonsoft.Json;

namespace GryphonUtility.Bot.Web.Models.Save
{
    internal sealed class Data
    {
        [JsonProperty]
        public SortedSet<Article> Articles { get; set; } = new SortedSet<Article>(new ArticleComparer());
    }
}