using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoreLinq;
using GryphonUtilityBot.Logic;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.ReplyMarkups;

namespace GryphonUtilityBot.Bot.Console
{
    internal sealed class Logic
    {
        public readonly TelegramBotClient Bot;

        private readonly int _masterId;

        private readonly IReadOnlyList<Item> _allItems;

        private readonly ReplyKeyboardMarkup _keyboard;

        private Queue<Item> _items;
        private Dictionary<Item, int> _itemAmounts;
        private Item _currentItem;
        private bool _currentAmountIsPacks;

        public Logic(Configuration config)
        {
            Bot = new TelegramBotClient(config.Token);
            Bot.OnMessage += OnMessageRecieved;

            _keyboard = GetKeyboard();

            _masterId = config.MasterId;
            _allItems = config.Items;
        }

        private async void OnMessageRecieved(object sender, MessageEventArgs e)
        {
            long chatId = e.Message.Chat.Id;
            if (e.Message.From.Id != _masterId)
            {
                await Bot.SendTextMessageAsync(chatId, "Unauthorized!");
                return;
            }

            if (e.Message.Text == Start)
            {
                Reset();
                await Bot.SendTextMessageAsync(chatId, "Сейчас есть:");
            }
            else
            {
                bool parced = int.TryParse(e.Message.Text, out int amount);
                if (!parced || (_currentItem == null))
                {
                    await Bot.SendTextMessageAsync(chatId, "Unknown command!");
                    return;
                }

                Add(amount);
            }

            if (_items.Count > 0)
            {
                string question = PrepareQuestion();
                await Bot.SendTextMessageAsync(chatId, question, replyMarkup: _keyboard);
            }
            else
            {
                string result = PrepareResult();
                await Bot.SendTextMessageAsync(chatId, result, replyMarkup: NoKeyboard);
            }
        }

        private void Reset()
        {
            _items = new Queue<Item>(_allItems.OrderBy(i => i.AskOrder));
            _itemAmounts = new Dictionary<Item, int>();
            _currentItem = null;
            _currentAmountIsPacks = false;
        }

        private void Add(int amount)
        {
            if (_currentAmountIsPacks)
            {
                _itemAmounts[_currentItem] = _currentItem.PackSize * amount;
            }
            else
            {
                if (_itemAmounts.ContainsKey(_currentItem))
                {
                    _itemAmounts[_currentItem] += amount;
                }
                else
                {
                    _itemAmounts[_currentItem] = amount;
                }
            }
        }

        private string PrepareQuestion()
        {
            if (_currentAmountIsPacks)
            {
                _currentAmountIsPacks = false;
                return $"{_currentItem.Name}, штуки:";
            }

            _currentItem = _items.Dequeue();
            if (_currentItem.PackSize == 1)
            {
                return $"{_currentItem.Name}:";
            }

            _currentAmountIsPacks = true;
            return $"{_currentItem.Name}, пачки:";
        }

        private string PrepareResult()
        {
            int days = GetDaysBeforeNextSunday();
            var sb = new StringBuilder();
            sb.AppendLine($"На {days} дней нужно докупить:");
            sb.AppendLine();
            foreach (Item item in _itemAmounts.Keys.OrderBy(i => i.ResultOrder))
            {
                int need = item.GetRefillingAmount(_itemAmounts[item], days);
                string packsPart = item.PackSize > 1 ? ", пачки" : "";
                sb.AppendLine($"{item.Name}{packsPart}: {need}");
            }
            return sb.ToString();
        }

        private static int GetDaysBeforeNextSunday()
        {
            return 8 + (7 + (DayOfWeek.Sunday - DateTime.Today.DayOfWeek)) % 7;
        }

        private static ReplyKeyboardMarkup GetKeyboard()
        {
            IEnumerable<KeyboardButton> buttons = Enumerable.Range(0, ButtonsTotal).Select(CreateButton);

            IEnumerable<IEnumerable<KeyboardButton>> keyboard = buttons.Batch(ButtonsPerRaw);

            return new ReplyKeyboardMarkup(keyboard);
        }

        private static KeyboardButton CreateButton(int option) => new KeyboardButton(option.ToString());

        private static readonly ReplyKeyboardRemove NoKeyboard = new ReplyKeyboardRemove();
        private const string Start = "/start";
        private const int ButtonsTotal = 12;
        private const int ButtonsPerRaw = 4;
    }
}
