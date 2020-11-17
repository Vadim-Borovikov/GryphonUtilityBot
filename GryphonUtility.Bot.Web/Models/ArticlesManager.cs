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
        internal ArticlesManager(long masterChatId, Manager saveManager, TimeSpan delay)
        {
            _masterChatId = masterChatId;
            _saveManager = saveManager;
            _delay = delay;
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

            if (message.ReplyToMessage != null)
            {
                await DeleteArticle(message.ReplyToMessage, client);
            }
            else
            {
                if (!TryParseArticle(message.Text, out Article article))
                {
                    return;
                }

                AddArticle(article, message.Chat, client);
            }
            await client.DeleteMessageAsync(message.Chat, message.MessageId);
        }

        private Task DeleteArticle(Message message, ITelegramBotClient client)
        {
            Article article = _saveManager.Data.Articles.FirstOrDefault(a => a.MessageId == message.MessageId);
            if (article == null)
            {
                return Task.CompletedTask;
            }

            _saveManager.Data.Articles.Remove(article);
            _saveManager.Data.Messages.Remove(message.MessageId);
            _saveManager.Save();
            return client.DeleteMessageAsync(message.Chat, message.MessageId);
        }

        private async void AddArticle(Article newArticle, Chat chat, ITelegramBotClient client)
        {
            _adding = true;

            _saveManager.Data.Articles.Add(newArticle);

            var messageIds = new Queue<int>(_saveManager.Data.Messages.Keys);
            foreach (Article article in _saveManager.Data.Articles.OrderByDescending(a => a.Date))
            {
                int messageId;
                string text = GetArticleMessageText(article);

                if (messageIds.Count > 0)
                {
                    messageId = messageIds.Dequeue();

                    if (_saveManager.Data.Messages[messageId] == text)
                    {
                        continue;
                    }

                    await Delay();
                    await client.EditMessageTextAsync(chat, messageId, text);
                }
                else
                {
                    await Delay();
                    Message message = await client.SendTextMessageAsync(chat, text);
                    messageId = message.MessageId;
                }

                article.MessageId = messageId;
                _saveManager.Data.Messages[messageId] = text;
            }

            _saveManager.Save();

            _adding = false;
        }

        private async Task Delay()
        {
            if (_delayedAt.HasValue)
            {
                TimeSpan delay = _delay - (DateTime.Now - _delayedAt.Value);
                if (delay.TotalMilliseconds > 0)
                {
                    await Task.Delay(delay);
                }
            }
            _delayedAt = DateTime.Now;
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

        private static string GetArticleMessageText(Article article)
        {
            return $"{article.Date:d MMMM yyyy}{Environment.NewLine}{article.Uri}";
        }

        private readonly long _masterChatId;
        private readonly Manager _saveManager;
        private readonly TimeSpan _delay;

        private bool _adding;
        private DateTime? _delayedAt;
    }
}
