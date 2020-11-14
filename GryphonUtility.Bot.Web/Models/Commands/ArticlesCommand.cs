using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GryphonUtility.Bot.Web.Models.Config;
using GryphonUtility.Bot.Web.Models.Save;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtility.Bot.Web.Models.Commands
{
    internal sealed class ArticlesCommand : Command
    {
        protected override string Name => "articles";

        public ArticlesCommand(IEnumerable<Article> articles, ChatId channelChatId, int firstMessageId,
            Manager saveManager, TimeSpan delay)
        {
            _articles = articles.OrderByDescending(a => a.Date).ToList();
            _channelChatId = channelChatId;
            _firstMessageId = firstMessageId;
            _saveManager = saveManager;
            _delay = delay;
        }

        internal override async Task ExecuteAsync(Message message, ITelegramBotClient client)
        {
            _saveManager.Load();

            Message statusMessage =
                await client.SendTextMessageAsync(message.Chat, "_Обновляю канал…_", ParseMode.Markdown);

            int updateMessages = _saveManager.Data.LastMassageId - _firstMessageId + 1;

            int messageId = _firstMessageId - 1;
            foreach (string text in _articles.Take(updateMessages).Select(GetArticleMessageText))
            {
                ++messageId;

                if (_saveManager.Data.Messages.ContainsKey(messageId)
                    && (_saveManager.Data.Messages[messageId] == text))
                {
                    continue;
                }

                await Delay();
                await client.EditMessageTextAsync(_channelChatId, messageId, text, ParseMode.Markdown);
                _saveManager.Data.Messages[messageId] = text;
            }

            foreach (string text in _articles.Skip(updateMessages).Select(GetArticleMessageText))
            {
                await Delay();
                Message newMessage = await client.SendTextMessageAsync(_channelChatId, text, ParseMode.Markdown);
                _saveManager.Data.Messages[newMessage.MessageId] = text;
            }

            _saveManager.Data.LastMassageId += _articles.Count - updateMessages;

            _saveManager.Save();

            await client.EditMessageTextAsync(message.Chat, statusMessage.MessageId, $"_{statusMessage.Text}_ Готово.",
                ParseMode.Markdown);
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

        private static string GetArticleMessageText(Article article) => $"[{article.Date:d MMMM yyyy}]({article.Uri})";

        private readonly IList<Article> _articles;
        private readonly ChatId _channelChatId;
        private readonly int _firstMessageId;
        private readonly Manager _saveManager;
        private readonly TimeSpan _delay;

        private DateTime? _delayedAt;
    }
}
