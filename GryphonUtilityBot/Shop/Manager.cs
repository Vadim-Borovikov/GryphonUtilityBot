﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GryphonUtilityBot.Shop
{
    internal sealed class Manager
    {
        public Manager(Bot.Bot bot)
        {
            _bot = bot;

            IEnumerable<KeyboardButton> buttons = Enumerable.Range(0, ButtonsTotal).Select(CreateButton);
            IEnumerable<IEnumerable<KeyboardButton>> keyboard = buttons.Batch(ButtonsPerRaw);
            _amountKeyboard = new ReplyKeyboardMarkup(keyboard);
        }

        public async Task ResetAndStartAskingAsync(ChatId chatId)
        {
            _items = new Queue<Item>(_bot.Config.Items.Where(i => !i.FixedNeed.HasValue).OrderBy(i => i.AskOrder));

            _itemAmounts = _bot.Config.Items.Where(i => i.FixedNeed.HasValue).ToDictionary(i => i, i => 0);

            _currentItem = null;
            _currentAmountIsPacks = false;

            await _bot.Client.SendTextMessageAsync(chatId, "Сейчас есть:");

            await InvokeNextActionAsync(chatId);
        }

        public Task ProcessNumberAsync(ChatId chatId, int number)
        {
            if (_currentItem == null)
            {
                return _bot.Client.SendTextMessageAsync(chatId, "Продукт не задан!");
            }

            Add(number);

            return InvokeNextActionAsync(chatId);
        }

        private static KeyboardButton CreateButton(int option) => new KeyboardButton(option.ToString());

        private void Add(int amount)
        {
            if (_currentAmountIsPacks)
            {
                amount *= _currentItem.PackSize;
            }

            if (_itemAmounts.ContainsKey(_currentItem))
            {
                _itemAmounts[_currentItem] += amount;
            }
            else
            {
                _itemAmounts[_currentItem] = amount;
            }
        }

        private Task InvokeNextActionAsync(ChatId chatId)
        {
            if (_currentAmountIsPacks || (_items.Count > 0))
            {
                string question = PrepareQuestion();
                return _bot.Client.SendTextMessageAsync(chatId, question, replyMarkup: _amountKeyboard);
            }

            _currentItem = null;
            string result = PrepareResult();
            return _bot.Client.SendTextMessageAsync(chatId, result, disableWebPagePreview: true, replyMarkup: NoKeyboard);
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
            foreach (Item item in _itemAmounts.Keys.OrderBy(i => i.ResultOrder))
            {
                int need = item.GetRefillingAmount(_itemAmounts[item], days);
                if (need == 0)
                {
                    continue;
                }
                if (item.HasHalves)
                {
                    int need2 = need / 2;
                    int need1 = need - need2;
                    sb.AppendLine($"{item.Half1}: {need1}");
                    sb.AppendLine(item.UriHalf1.AbsoluteUri);
                    if (need2 > 0)
                    {
                        sb.AppendLine();
                        sb.AppendLine($"{item.Half2}: {need2}");
                        sb.AppendLine(item.UriHalf2.AbsoluteUri);
                    }
                }
                else
                {
                    if (item.Mass.HasValue)
                    {
                        decimal mass = GetRefillingMass(item.Mass.Value, need);
                        sb.AppendLine($"{item.Name}: {mass.ToString(CultureInfo.InvariantCulture)} кг.");
                    }
                    else
                    {
                        sb.AppendLine($"{item.Name}: {need}");
                    }
                    if (item.Uri != null)
                    {
                        sb.AppendLine(item.Uri.AbsoluteUri);
                    }
                }
                sb.AppendLine();
            }
            string result = sb.ToString();
            if (string.IsNullOrWhiteSpace(result))
            {
                result = "А ничего и не надо!";
            }
            return result;
        }

        private static int GetDaysBeforeNextSunday() => 8 + (7 + (DayOfWeek.Sunday - DateTime.Today.DayOfWeek)) % 7;
        private static decimal GetRefillingMass(decimal mass, int amount) => Math.Ceiling(mass * amount * 10) / 10;

        private const int ButtonsTotal = 12;
        private const int ButtonsPerRaw = 4;
        private static readonly ReplyKeyboardRemove NoKeyboard = new ReplyKeyboardRemove();

        private readonly ReplyKeyboardMarkup _amountKeyboard;

        private Queue<Item> _items;
        private Dictionary<Item, int> _itemAmounts;
        private Item _currentItem;
        private bool _currentAmountIsPacks;

        private readonly Bot.Bot _bot;
    }
}
