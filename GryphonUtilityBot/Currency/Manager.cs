using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GryphonUtilityBot.Currency;

internal sealed class Manager
{
    private static bool IsRuble(Info.Currecny currecny)
    {
        return (currecny == Info.Currecny.RURCurrent) || (currecny == Info.Currecny.RURBefore);
    }

    public Manager(Bot bot)
    {
        _bot = bot;
        _current = Info.Currecny.AED;
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
        Info info = CurrencyInfos[_current];
        builder.AppendLine($"{amount:N0} {info.Code} — это:");
        foreach (Info.Currecny c in CurrencyInfos.Keys)
        {
            if (c == _current)
            {
                // continue;
            }

            if (IsRuble(c) && IsRuble(_current))
            {
                continue;
            }

            Info i = CurrencyInfos[c];
            decimal a = amount * i.ToUSD / info.ToUSD;
            builder.AppendLine($"• {a:N0} {i.Code}");
        }
        return builder.ToString();
    }

    private const int ButtonsTotal = 12;
    private const int ButtonsPerRaw = 4;
    private static readonly ReplyKeyboardRemove NoKeyboard = new ReplyKeyboardRemove();

    private readonly ReplyKeyboardMarkup _amountKeyboard;

    private Info.Currecny _current;
    private static readonly Dictionary<Info.Currecny, Info> CurrencyInfos =
        new()
        {
            {
                Info.Currecny.RURCurrent,
                new Info(60, "₽ (сейчас)", "RUR ₽, российские рубли сейчас")
            },
            {
                Info.Currecny.RURBefore,
                new Info(75, "₽ (раньше)", "RUR ₽, российские рубли до войны")
            },
            { Info.Currecny.USD, new Info(1, "$", "USD $, доллары США")  },
            { Info.Currecny.AED, new Info(3.5m, "AED", "AED, дирхамы ОАЭ")  },
            { Info.Currecny.TRY, new Info(17.5m, "₺", "TRY ₺, турецкие лиры")  }
        };

    private readonly Bot _bot;
}