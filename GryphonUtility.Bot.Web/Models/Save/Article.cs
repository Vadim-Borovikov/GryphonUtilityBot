using System;
using Newtonsoft.Json;

namespace GryphonUtility.Bot.Web.Models.Save
{
    internal sealed class Article
    {
        [JsonProperty]
        public Uri Uri { get; set; }
        [JsonProperty]
        public DateTime Date { get; set; }

        public Article(DateTime date, Uri uri)
        {
            Date = date;
            Uri = uri;
        }
    }
}