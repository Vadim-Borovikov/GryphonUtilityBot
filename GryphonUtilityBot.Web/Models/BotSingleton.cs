using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using GryphonUtilities.Extensions;

namespace GryphonUtilityBot.Web.Models;

public sealed class BotSingleton : IDisposable
{
    internal readonly Bot Bot;

    public BotSingleton(Config config)
    {
        Bot = new Bot(config);
        PingUrls = GetPingUrls(config).ToList();
    }

    public void Dispose() => Bot.Dispose();

    internal readonly List<Uri> PingUrls;

    private static IEnumerable<Uri> GetPingUrls(Config config)
    {
        if (config.PingUrls is not null && (config.PingUrls.Count != 0))
        {
            return config.PingUrls.RemoveNulls();
        }

        if (config.PingUrlsJson is not null)
        {
            List<Uri>? deserialized = JsonSerializer.Deserialize<List<Uri>>(config.PingUrlsJson);
            if (deserialized is not null)
            {
                return deserialized;
            }
        }

        return Array.Empty<Uri>();
    }
}