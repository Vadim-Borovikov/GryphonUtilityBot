using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GryphonUtilityBot.Logic;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GryphonUtilityBot.Bot.Console.Commands
{
    internal sealed class ArticlesCommand : Command
    {
        protected override string Name => "articles";

        public ArticlesCommand(IEnumerable<Article> articles, ChatId channelChatId, int firstMessageId,
            BotSaveManager saveManager, TimeSpan delay)
        {
            _articles = articles.OrderBy(a => a.Date).ToList();
            _channelChatId = channelChatId;
            _firstMessageId = firstMessageId;
            _saveManager = saveManager;
            _delay = delay;
        }

        internal override async Task ExecuteAsync(ChatId chatId, ITelegramBotClient client)
        {
            _saveManager.Load();

            Message statusMessage =
                await client.SendTextMessageAsync(chatId, "_Обновляю канал…_", ParseMode.Markdown);

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

                await client.EditMessageTextAsync(_channelChatId, messageId, text, ParseMode.Markdown);
                await Task.Delay(_delay);
                _saveManager.Data.Messages[messageId] = text;
            }

            foreach (string text in _articles.Skip(updateMessages).Select(GetArticleMessageText))
            {
                await client.SendTextMessageAsync(_channelChatId, text, ParseMode.Markdown);
                await Task.Delay(_delay);
            }

            _saveManager.Data.LastMassageId += _articles.Count - updateMessages;

            _saveManager.Save();

            await client.EditMessageTextAsync(chatId, statusMessage.MessageId, $"_{statusMessage.Text}_ Готово.",
                ParseMode.Markdown);
        }

        private static string GetArticleMessageText(Article article)
        {
            return $"{article.Date:d MMMM yyyy}{Environment.NewLine}[{article.Name}]({article.Uri})";
        }

        private readonly IList<Article> _articles;
        private readonly ChatId _channelChatId;
        private readonly int _firstMessageId;
        private readonly BotSaveManager _saveManager;
        private readonly TimeSpan _delay;
    }
}
