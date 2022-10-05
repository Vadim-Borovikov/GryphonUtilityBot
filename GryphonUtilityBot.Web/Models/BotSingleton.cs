namespace GryphonUtilityBot.Web.Models;

public sealed class BotSingleton
{
    internal readonly Bot Bot;

    public BotSingleton(Config config)
    {
        Bot = new Bot(config);
    }
}