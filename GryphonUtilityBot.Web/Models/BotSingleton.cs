using System;
using System.Collections.Generic;
using GryphonUtilities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Web.Models;

public sealed class BotSingleton : IDisposable
{
    internal readonly Bot Bot;

    public BotSingleton(IOptions<ConfigJson> options)
    {
        ConfigJson configJson = options.Value;
        Config config = configJson.Convert();
        Bot = new Bot(config);

        if (configJson.PingUrls is null || (configJson.PingUrls.Count == 0))
        {
            string json = configJson.PingUrlsJson.GetValue(nameof(configJson.PingUrlsJson));
            configJson.PingUrls = JsonConvert.DeserializeObject<List<Uri?>>(json);
        }
        if (configJson.PingUrls is not null)
        {
            PingUrls.AddRange(configJson.PingUrls.RemoveNulls());
        }
    }

    public void Dispose() => Bot.Dispose();

    internal readonly List<Uri> PingUrls = new();
}