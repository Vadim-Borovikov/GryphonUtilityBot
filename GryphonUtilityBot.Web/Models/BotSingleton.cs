using Microsoft.Extensions.Options;

namespace GryphonUtilityBot.Web.Models;

public sealed class BotSingleton
{
    internal readonly Bot Bot;

    public BotSingleton(IOptions<ConfigJson> options)
    {
        ConfigJson configJson = options.Value;
        Config config = configJson.Convert();
        Bot = new Bot(config);
    }
}