using System.IO;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types;

namespace GryphonUtility.Bot.Console
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            System.Console.WriteLine("Reading config...");

            Configuration config = GetConfig();

            var bot = new Bot(config);

            User me = bot.Client.GetMeAsync().Result;
            System.Console.Title = me.Username;

            bot.Client.StartReceiving();
            System.Console.WriteLine($"Start listening for @{me.Username}");
            System.Console.ReadLine();
            bot.Client.StopReceiving();
        }

        private static Configuration GetConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.override.json") // Create appsettings.override.json for private settings
                .Build()
                .Get<Configuration>();
        }
    }
}
