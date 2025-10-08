using GryphonUtilities;
using GryphonUtilities.Time;
using GryphonUtilityBot.Web.Models;
using GryphonUtilityBot.Web.Models.Calendar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GryphonUtilityBot.Configs;

namespace GryphonUtilityBot.Web;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        Logger.DeleteExceptionLog();
        Clock clock = new();
        Logger logger = new(clock);
        try
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            Models.Config config = Configure(builder) ?? throw new NullReferenceException("Can't load config.");
            clock = new Clock(config.SystemTimeZoneIdLogs);
            logger = new Logger(clock);
            logger.LogStartup();

            IServiceCollection services = builder.Services;
            services.AddControllersWithViews(AddExceptionFilter);
            services.ConfigureTelegramBotMvc();

            services.AddSingleton(logger);

            Bot bot = await Bot.TryCreateAsync(config, CancellationToken.None)
                                                                ?? throw new InvalidOperationException("Failed to initialize bot due to invalid configuration.");
            services.AddSingleton(bot);
            services.AddHostedService<BotService>();

            AddCalendarTo(services, config);

            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors();

            UseUpdateEndpoint(app, config.Token);

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            logger.LogException(ex);
        }
    }

    private static Models.Config? Configure(WebApplicationBuilder builder)
    {
        ConfigurationManager configuration = builder.Configuration;
        Models.Config? config = configuration.Get<Models.Config>();
        if (config is null)
        {
            return null;
        }

        builder.Services.AddOptions<Models.Config>().Bind(configuration).ValidateDataAnnotations();
        builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<Models.Config>>().Value);

        LoadTextsFile(builder, config);

        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(config.CultureInfoName);

        return config;
    }

    private static void LoadTextsFile(WebApplicationBuilder builder, Configs.Config config)
    {
        string? file = Directory.GetFiles(builder.Environment.ContentRootPath, "texts.*.json").SingleOrDefault();
        if (file is null)
        {
            return;
        }

        builder.Configuration.AddJsonFile(file, true, true);

        Texts? texts = builder.Configuration.Get<Texts>();
        if (texts is not null)
        {
            config.Texts = texts;
        }
    }

    private static void AddExceptionFilter(MvcOptions options) => options.Filters.Add<GlobalExceptionFilter>();

    private static void AddCalendarTo(IServiceCollection services, Models.Config config)
    {
        services.AddNotionClient(options => options.AuthToken = config.NotionToken);
        services.AddSingleton<Models.Calendar.Notion.Provider>();
        services.AddSingleton<GoogleCalendarProvider>();
        services.AddSingleton<IUpdatesSubscriber, Synchronizer>();
    }

    private static void UseUpdateEndpoint(IApplicationBuilder app, string token)
    {
        object defaults = new
        {
            controller = "Update",
            action = "Post"
        };
        app.UseEndpoints(endpoints => endpoints.MapControllerRoute("update", token, defaults));
    }
}