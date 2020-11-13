using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types;

namespace GryphonUtilityBot.Bot.Console
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            System.Console.WriteLine("Reading config...");

            Configuration config = GetConfig();

            var logic = new Logic(config);

            User me = logic.Bot.GetMeAsync().Result;
            System.Console.Title = me.Username;

            logic.Bot.StartReceiving();
            System.Console.WriteLine($"Start listening for @{me.Username}");
            System.Console.ReadLine();
            logic.Bot.StopReceiving();
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
