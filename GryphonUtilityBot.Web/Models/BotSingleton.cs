using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Web.Models
{
    public sealed class BotSingleton
    {
        internal readonly Bot Bot;

        public BotSingleton(IOptions<Config> options)
        {
            Config config = options.Value;

            if ((config.AdminIds == null) || (config.AdminIds.Count == 0))
            {
                config.AdminIds = JsonConvert.DeserializeObject<List<long>>(config.AdminIdsJson);
            }
            Bot = new Bot(config);
        }
    }
}