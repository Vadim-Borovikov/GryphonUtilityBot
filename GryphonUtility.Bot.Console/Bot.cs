using System.Collections.Generic;
using System.Linq;
using GryphonUtility.Bot.Console.Commands;
using Telegram.Bot;
using Telegram.Bot.Args;

namespace GryphonUtility.Bot.Console
{
    internal sealed class Bot
    {
        public readonly TelegramBotClient Client;

        private readonly int _masterId;

        private readonly List<Command> _commands;
        private readonly ShopCommand _shopCommand;

        public Bot(Configuration config)
        {
            Client = new TelegramBotClient(config.Token);

            var saveManager = new BotSaveManager(config.SavePath);

            _shopCommand = new ShopCommand(config.Items);

            _commands = new List<Command>
            {
                _shopCommand,
                new ArticlesCommand(config.Articles, config.ArticlesChannelChatId, config.ArticlesFirstMessageId,
                saveManager, config.Delay)
            };

            Client.OnMessage += OnMessageRecieved;

            _masterId = config.MasterId;
        }

        private async void OnMessageRecieved(object sender, MessageEventArgs e)
        {
            long chatId = e.Message.Chat.Id;
            if (e.Message.From.Id != _masterId)
            {
                await Client.SendTextMessageAsync(chatId, "Unauthorized!");
                return;
            }

            Command command = _commands.FirstOrDefault(c => c.Contains(e.Message));
            if (command != null)
            {
                await command.ExecuteAsync(chatId, Client);
                return;
            }

            if (int.TryParse(e.Message.Text, out int number))
            {
                await _shopCommand.ProcessNumberAsync(chatId, Client, number);
                return;
            }

            await Client.SendTextMessageAsync(chatId, "Unknown command!");
        }
    }
}
