using System;
using System.Collections.Generic;
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
        internal ArticlesManager(long masterChatId, Manager saveManager, int messagesPerMinuteLimit)
        {
            _masterChatId = masterChatId;
            _saveManager = saveManager;
            _messagesPerMinuteLimit = messagesPerMinuteLimit;
            _adding = false;
            _saveManager.Load();
        }

        internal async Task ProcessChannelMessageAsync(Message message, ITelegramBotClient client)
        {
            if (_adding)
            {
                await client.SendTextMessageAsync(_masterChatId,
                    $"declined channel update:{Environment.NewLine}`{message.Text}`", ParseMode.Markdown);
                return;
            }

            if (message.ReplyToMessage == null)
            {
                if (!TryParseArticle(message.Text, out Article article))
                {
                    return;
                }

                AddArticle(article, message.Chat, client);
            }
            else
            {
                await DeleteArticle(message.ReplyToMessage, client);
            }

            await client.DeleteMessageAsync(message.Chat, message.MessageId);
        }

        private static bool TryParseArticle(string text, out Article article)
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

        private async void AddArticle(Article newArticle, ChatId chatId, ITelegramBotClient client)
        {
            _adding = true;

            List<Article> toUpdate = _saveManager.Data.Articles
                .Where(a => a.Date < newArticle.Date)
                .ToList();

            bool delayNeeded = (toUpdate.Count + 1) >= _messagesPerMinuteLimit;

            foreach (Article article in toUpdate)
            {
                Article oldArticle = article.Copy();

                article.Date = newArticle.Date;
                article.Uri = newArticle.Uri;

                await SendOrEditAsync(delayNeeded, client, article, chatId, article.MessageId);

                newArticle = oldArticle;
            }

            Message message = await SendOrEditAsync(delayNeeded, client, newArticle, chatId);
            newArticle.MessageId = message.MessageId;

            _saveManager.Data.Articles.Add(newArticle);

            _saveManager.Save();

            _adding = false;
        }

        private Task DeleteArticle(Message message, ITelegramBotClient client)
        {
            Article article = _saveManager.Data.Articles.FirstOrDefault(a => a.MessageId == message.MessageId);
            if (article == null)
            {
                return Task.CompletedTask;
            }

            _saveManager.Data.Articles.Remove(article);
            _saveManager.Save();
            return client.DeleteMessageAsync(message.Chat, message.MessageId);
        }

        private async Task<Message> SendOrEditAsync(bool delayNeeded, ITelegramBotClient client, Article article,
            ChatId chatId, int? messageId = null)
        {
            if (delayNeeded)
            {
                await Delay();
            }

            string text = GetArticleMessageText(article);

            if (messageId.HasValue)
            {
                return await client.EditMessageTextAsync(chatId, messageId.Value, text);
            }

            return await client.SendTextMessageAsync(chatId, text);
        }

        private async Task Delay()
        {
            if (_delayedAt.HasValue)
            {
                TimeSpan delay =
                    TimeSpan.FromMinutes(1.0 / _messagesPerMinuteLimit) - (DateTime.Now - _delayedAt.Value);
                if (delay.TotalMilliseconds > 0)
                {
                    await Task.Delay(delay);
                }
            }
            _delayedAt = DateTime.Now;
        }

        private static string GetArticleMessageText(Article article)
        {
            return $"{article.Date:d MMMM yyyy}{Environment.NewLine}{article.Uri}";
        }

        private readonly long _masterChatId;
        private readonly Manager _saveManager;
        private readonly int _messagesPerMinuteLimit;

        private bool _adding;
        private DateTime? _delayedAt;
    }
}
