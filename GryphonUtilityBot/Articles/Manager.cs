using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4;
using GoogleSheetsManager;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Articles
{
    internal sealed class Manager
    {
        public Manager(Bot.Bot bot)
        {
            _bot = bot;
            _articles = new SortedSet<Article>();
        }

        public static bool TryParseArticle(string text, out Article article)
        {
            article = ParseArticle(text);
            return article != null;
        }

        public async Task ProcessNewArticleAsync(ChatId chatId, Article article)
        {
            AddArticle(article);

            string articleText = GetArticleMessageText(article);
            await _bot.Client.SendTextMessageAsync(chatId, $"Добавлено: `{articleText}`.", ParseMode.Markdown);
            await SendFirstArticleAsync(chatId);
        }

        public Task SendFirstArticleAsync(ChatId chatId)
        {
            Load();

            string text = GetArticleMessageText(_articles.First());
            return _bot.Client.SendTextMessageAsync(chatId, text);
        }

        public async Task DeleteFirstArticleAsync(ChatId chatId)
        {
            Load();

            Article article = _articles.First();
            _articles.Remove(article);

            Save();

            string articleText = GetArticleMessageText(article);
            await _bot.Client.SendTextMessageAsync(chatId, $"Удалено: `{articleText}`.", ParseMode.Markdown);
            await SendFirstArticleAsync(chatId);
        }

        private static Article ParseArticle(string text)
        {
            string[] parts = text.Split(' ');

            Uri uri;
            switch (parts.Length)
            {
                case 1:
                    uri = CreateUri(parts[0]);
                    return uri == null ? null : new Article(DateTime.Today, uri);
                case 2:
                    DateTime? date = ParseDate(parts[0]);
                    if (!date.HasValue)
                    {
                        return null;
                    }
                    uri = CreateUri(parts[1]);
                    return uri == null ? null : new Article(date.Value, uri);
                default: return null;
            }
        }

        private static DateTime? ParseDate(string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime date))
            {
                return date;
            }

            if (!int.TryParse(dateString, out int day))
            {
                return null;
            }

            try
            {
                return new DateTime(DateTime.Today.Year, DateTime.Today.Month, day);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        private static Uri CreateUri(string uriString)
        {
            return Uri.TryCreate(uriString, UriKind.Absolute, out Uri uri) ? uri : null;
        }

        private void AddArticle(Article article)
        {
            Load();

            _articles.Add(article);

            Save();
        }

        private void Load()
        {
            IList<Article> articles =
                DataManager.GetValues<Article>(_bot.GoogleSheetsProvider, _bot.Config.GoogleRange,
                    SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.UNFORMATTEDVALUE);
            _articles = new SortedSet<Article>(articles);
        }

        private void Save()
        {
            _bot.GoogleSheetsProvider.ClearValues(_bot.Config.GoogleRange);
            DataManager.UpdateValues(_bot.GoogleSheetsProvider, _bot.Config.GoogleRange, _articles);
        }

        private static string GetArticleMessageText(Article article)
        {
            return $"{article.Date:d MMMM yyyy}{Environment.NewLine}{article.Uri}";
        }

        private SortedSet<Article> _articles;
        private readonly Bot.Bot _bot;
    }
}
