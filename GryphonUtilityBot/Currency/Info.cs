using Telegram.Bot.Types.ReplyMarkups;

namespace GryphonUtilityBot.Currency;

internal sealed class Info
{
    public enum Currecny
    {
        RURCurrent,
        RURBefore,
        USD,
        AED,
        TRY
    }

    public readonly decimal ToUSD;
    public readonly string Code;
    public readonly InlineKeyboardButton Button;

    public Info(decimal toUSD, string code, string description)
    {
        ToUSD = toUSD;
        Code = code;
        Button = new InlineKeyboardButton(description)
        {
            CallbackData = code
        };
    }
}