namespace GryphonUtilityBot.Currency;

internal sealed class CurrencyInfo
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
    public readonly string Description;

    public CurrencyInfo(decimal toUSD, string code, string description)
    {
        ToUSD = toUSD;
        Code = code;
        Description = description;
    }
}