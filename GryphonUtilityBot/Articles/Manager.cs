using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            article = null;
            string[] parts = text.Split(' ');
            if (parts.Length != 2)
            {
                return false;
            }

            if (!DateTime.TryParse(parts[0], out DateTime date))
            {
                if (!int.TryParse(parts[0], out int day))
                {
                    return false;
                }

                try
                {
                    date = new DateTime(DateTime.Today.Year, DateTime.Today.Month, day);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return false;
                }
            }

            if (!Uri.TryCreate(parts[1], UriKind.Absolute, out Uri uri))
            {
                return false;
            }

            article = new Article(date, uri);
            return true;
        }

        public Task ProcessNewArticleAsync(ChatId chatId, Article article)
        {
            AddArticle(article);

            string articleText = GetArticleMessageText(article);
            string firstArticleText = GetArticleMessageText(_articles.First());

            var sb = new StringBuilder();
            sb.AppendLine($"Добавлено: `{articleText}`.");
            sb.AppendLine();
            sb.AppendLine($"Первая статья: {firstArticleText}");

            return _bot.Client.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Markdown);
        }

        public Task SendFirstArticleAsync(ChatId chatId)
        {
            Load();

            string text = GetArticleMessageText(_articles.First());
            return _bot.Client.SendTextMessageAsync(chatId, text);
        }

        public Task DeleteFirstArticleAsync(ChatId chatId)
        {
            Load();

            Article article = _articles.First();
            string articleText = GetArticleMessageText(article);

            _articles.Remove(article);

            Save();

            string firstArticleText = GetArticleMessageText(_articles.First());

            var sb = new StringBuilder();
            sb.AppendLine($"Удалено: `{articleText}`.");
            sb.AppendLine();
            sb.AppendLine($"Первая статья: {firstArticleText}");

            return _bot.Client.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Markdown);
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
                DataManager.GetValues<Article>(_bot.GoogleSheetsProvider, _bot.Config.GoogleRange);
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
