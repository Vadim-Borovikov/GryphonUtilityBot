using System;
using System.Linq;
using System.Threading.Tasks;
using GryphonUtility.Bot.Web.Models.Save;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtility.Bot.Web.Models
{
    public sealed class ArticlesManager
    {
        internal ArticlesManager(Manager saveManager) { _saveManager = saveManager; }

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

        internal async Task AddArticleAsync(Article article, ChatId chatId, ITelegramBotClient client)
        {
            _saveManager.Load();

            _saveManager.Data.Articles.Add(article);

            _saveManager.Data.Articles = _saveManager.Data.Articles
                .OrderBy(a => a.Date)
                .ThenBy(a => a.Uri.AbsoluteUri)
                .ToList();
            _saveManager.Save();

            string text = GetArticleMessageText(article);
            await client.SendTextMessageAsync(chatId, $"Added `{text}`", ParseMode.Markdown);

            await SendOldestArticleAsync(chatId, client);
        }

        internal Task SendOldestArticleAsync(ChatId chatId, ITelegramBotClient client)
        {
            _saveManager.Load();

            string text = GetArticleMessageText(_saveManager.Data.Articles[0]);
            return client.SendTextMessageAsync(chatId, text);
        }

        internal void DeleteOldestArticle()
        {
            _saveManager.Load();

            _saveManager.Data.Articles.RemoveAt(0);

            _saveManager.Save();
        }

        private static string GetArticleMessageText(Article article)
        {
            return $"{article.Date:d MMMM yyyy}{Environment.NewLine}{article.Uri}";
        }

        private readonly Manager _saveManager;
    }
}
