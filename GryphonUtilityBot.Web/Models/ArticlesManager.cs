using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleSheetsManager;
using GryphonUtilityBot.Web.Models.Save;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Web.Models
{
    public sealed class ArticlesManager
    {
        internal ArticlesManager(Provider googleSheetsProvider, string range)
        {
            _googleSheetsProvider = googleSheetsProvider;
            _range = range;
            _articles = new SortedSet<Article>();
        }

        internal static bool TryParseArticle(string text, out Article article)
        {
            article = null;
            string[] parts = text.Split(' ');
            if (parts.Length != 2)
            {
                return false;
            }

            if (!DateTime.TryParse(parts[0], out DateTime date))
            {
                return false;
            }

            if (!Uri.TryCreate(parts[1], UriKind.Absolute, out Uri uri))
            {
                return false;
            }

            article = new Article(date, uri);
            return true;
        }

        internal Task ProcessNewArticleAsync(ITelegramBotClient client, ChatId chatId, Article article)
        {
            AddArticle(article);

            string articleText = GetArticleMessageText(article);
            string firstArticleText = GetArticleMessageText(_articles.First());

            var sb = new StringBuilder();
            sb.AppendLine($"Добавлено: `{articleText}`.");
            sb.AppendLine();
            sb.AppendLine($"Первая статья: {firstArticleText}");

            return client.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Markdown);
        }

        internal Task SendFirstArticleAsync(ITelegramBotClient client, ChatId chatId)
        {
            Load();

            string text = GetArticleMessageText(_articles.First());
            return client.SendTextMessageAsync(chatId, text);
        }

        internal Task DeleteFirstArticleAsync(ITelegramBotClient client, ChatId chatId)
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

            return client.SendTextMessageAsync(chatId, sb.ToString(), ParseMode.Markdown);
        }

        private void AddArticle(Article article)
        {
            Load();

            _articles.Add(article);

            Save();
        }

        private void Load()
        {
            IList<Article> articles = DataManager.GetValues<Article>(_googleSheetsProvider, _range);
            _articles = new SortedSet<Article>(articles);
        }

        private void Save()
        {
            _googleSheetsProvider.ClearValues(_range);
            DataManager.UpdateValues(_googleSheetsProvider, _range, _articles);
        }

        private static string GetArticleMessageText(Article article)
        {
            return $"{article.Date:d MMMM yyyy}{Environment.NewLine}{article.Uri}";
        }

        private SortedSet<Article> _articles;
        private readonly Provider _googleSheetsProvider;
        private readonly string _range;
    }
}
