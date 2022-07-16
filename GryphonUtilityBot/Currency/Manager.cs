using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GryphonUtilityBot.Currency;

internal sealed class Manager
{
    private static bool IsRuble(Info.Currecny currecny)
    {
        return currecny is Info.Currecny.RURCurrent or Info.Currecny.RURBefore;
    }

    public Manager(Bot bot)
    {
        _bot = bot;
        _currentCurrency = Info.Currecny.AED;
    }

    private static IEnumerable<InlineKeyboardButton> GetRow(Info currecny)
    {
        return new List<InlineKeyboardButton> { currecny.Button };
    }

    private static InlineKeyboardMarkup GetKeyboardWithout(Info.Currecny currecny)
    {
        IEnumerable<IEnumerable<InlineKeyboardButton>> keyboard =
            CurrencyInfos.Where(p => p.Key != currecny).Select(p => GetRow(p.Value));
        return new InlineKeyboardMarkup(keyboard);
    }

    public async Task ProcessNumberAsync(Chat chat, decimal number)
    {
        _currentAmount = number;
        string message = PrepareResult(_currentAmount);
        InlineKeyboardMarkup keyboard = GetKeyboardWithout(_currentCurrency);
        _currentMessage = await _bot.SendTextMessageAsync(chat, message, replyMarkup: keyboard);
    }

    public Task ChangeCurrency(string code)
    {
        if (_currentMessage is null)
        {
            return Task.CompletedTask;
        }
        _currentCurrency = CurrencyInfos.Single(p => p.Value.Code == code).Key;
        string message = PrepareResult(_currentAmount);
        InlineKeyboardMarkup keyboard = GetKeyboardWithout(_currentCurrency);
        return
            _bot.EditMessageTextAsync(_currentMessage.Chat, _currentMessage.MessageId, message, replyMarkup: keyboard);
    }


    private string PrepareResult(decimal amount)
    {
        StringBuilder builder = new();
        Info info = CurrencyInfos[_currentCurrency];
        builder.AppendLine($"{amount:N0} {info.Code} — это:");
        foreach (Info.Currecny c in CurrencyInfos.Keys)
        {
            if (c == _currentCurrency)
            {
                continue;
            }

            if (IsRuble(c) && IsRuble(_currentCurrency))
            {
                continue;
            }

            Info i = CurrencyInfos[c];
            decimal a = amount * i.ToUSD / info.ToUSD;
            builder.AppendLine($"• {a:N0} {i.Code}");
        }
        return builder.ToString();
    }

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
    private decimal _currentAmount;
    private Info.Currecny _currentCurrency;
    private Message? _currentMessage;
}