using System;
using Newtonsoft.Json;

namespace GryphonUtility.Bot.Web.Models.Save
{
    internal sealed class Article
    {
        [JsonProperty]
        public string Text { get; set; }
        [JsonProperty]
        public DateTime Date { get; set; }
        [JsonProperty]
        public int MessageId { get; set; }

        public Article(DateTime date, string text)
        {
            Date = date;
            Text = text;
        }

        public Article Copy() => new Article(Date, Text) { MessageId = MessageId };
    }
}