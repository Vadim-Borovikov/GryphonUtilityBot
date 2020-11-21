using System;
using System.Linq;
using System.Text;
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

        internal async Task ProcessNewArticleAsync(Article article, Message message, ITelegramBotClient client)
        {
            AddArticle(article);

            string articleText = GetArticleMessageText(article);
            string firstArticleText = GetArticleMessageText(_saveManager.Data.Articles.First());

            var sb = new StringBuilder();
            sb.AppendLine($"Added `{articleText}`.");
            sb.AppendLine($"First article: {firstArticleText}");

            await client.SendTextMessageAsync(message.Chat, sb.ToString(), ParseMode.Markdown);
        }

        internal Task SendFirstArticleAsync(ChatId chatId, ITelegramBotClient client)
        {
            _saveManager.Load();

            string text = GetArticleMessageText(_saveManager.Data.Articles.First());
            return client.SendTextMessageAsync(chatId, text);
        }

        internal void DeleteFirstArticle()
        {
            _saveManager.Load();

            Article first = _saveManager.Data.Articles.First();
            _saveManager.Data.Articles.Remove(first);

            _saveManager.Save();
        }

        private void AddArticle(Article article)
        {
            _saveManager.Load();

            _saveManager.Data.Articles.Add(article);

            _saveManager.Save();
        }

        private static string GetArticleMessageText(Article article)
        {
            return $"{article.Date:d MMMM yyyy}{Environment.NewLine}{article.Uri}";
        }

        private readonly Manager _saveManager;
    }
}
