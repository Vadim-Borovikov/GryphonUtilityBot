using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GryphonUtilityBot.Currency
{
    internal sealed class Manager
    {
        private static bool IsRuble(CurrencyInfo.Currecny currecny)
        {
            return (currecny == CurrencyInfo.Currecny.RURCurrent) || (currecny == CurrencyInfo.Currecny.RURBefore);
        }

        public Manager(Bot bot)
        {
            _bot = bot;
            _current = CurrencyInfo.Currecny.AED;
            /*IEnumerable<KeyboardButton> buttons = Enumerable.Range(0, ButtonsTotal).Select(CreateButton);
            IEnumerable<IEnumerable<KeyboardButton>> keyboard = buttons.Batch(ButtonsPerRaw);
            _amountKeyboard = new ReplyKeyboardMarkup(keyboard);*/
        }

        public Task ProcessNumberAsync(ChatId chatId, decimal number)
        {
            string message = PrepareResult(number);
            return _bot.Client.SendTextMessageAsync(chatId, message);
        }

        private static KeyboardButton CreateButton(int option) => new KeyboardButton(option.ToString());

        private string PrepareResult(decimal amount)
        {
            var builder = new StringBuilder();
            CurrencyInfo info = CurrencyInfos[_current];
            builder.AppendLine($"{amount} {info.Code} — это:");
            foreach (CurrencyInfo.Currecny c in CurrencyInfos.Keys)
            {
                if (c == _current)
                {
                    continue;
                }

                if (IsRuble(c) && IsRuble(_current))
                {
                    continue;
                }

                CurrencyInfo i = CurrencyInfos[c];
                decimal a = amount * info.ToUSD / i.ToUSD;
                builder.AppendLine($"• {a:#.##} {i.Code}");
            }
            return builder.ToString();
        }

        private const int ButtonsTotal = 12;
        private const int ButtonsPerRaw = 4;
        private static readonly ReplyKeyboardRemove NoKeyboard = new ReplyKeyboardRemove();

        private readonly ReplyKeyboardMarkup _amountKeyboard;

        private CurrencyInfo.Currecny _current;
        private static readonly Dictionary<CurrencyInfo.Currecny, CurrencyInfo> CurrencyInfos =
            new Dictionary<CurrencyInfo.Currecny, CurrencyInfo>
        {
            {
                CurrencyInfo.Currecny.RURCurrent,
                new CurrencyInfo(60, "RUR (сейчас)", "RUR ₽, российские рубли сейчас")
            },
            {
                CurrencyInfo.Currecny.RURBefore,
                new CurrencyInfo(75, "RUR (раньше)", "RUR ₽, российские рубли до войны")
            },
            { CurrencyInfo.Currecny.USD, new CurrencyInfo(1, "USD", "USD $, доллары США")  },
            { CurrencyInfo.Currecny.AED, new CurrencyInfo(3.5m, "AED", "AED, дирхамы ОАЭ")  },
            { CurrencyInfo.Currecny.TRY, new CurrencyInfo(17.5m, "TRY", "TRY ₺, турецкие лиры")  }
        };

        private readonly Bot _bot;
    }
}
