﻿using System;
using System.Globalization;
using System.Threading.Tasks;
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

            Config config = Configure(builder) ?? throw new NullReferenceException("Can't load config.");
            clock = new Clock(config.SystemTimeZoneIdLogs);
            logger = new Logger(clock);
            logger.LogStartup();

            IServiceCollection services = builder.Services;
            IMvcBuilder mvc = services.AddControllersWithViews(AddEsceptionFilter);
            mvc.AddNewtonsoftJson(); //  TODO remove after bot update

            services.AddSingleton(clock);
            services.AddSingleton(logger);

            AddBotTo(services);

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

    private static Config? Configure(WebApplicationBuilder builder)
    {
        ConfigurationManager configuration = builder.Configuration;
        Config? config = configuration.Get<Config>();
        if (config is null)
        {
            return null;
        }

        builder.Services.AddOptions<Config>().Bind(configuration).ValidateDataAnnotations();
        builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<Config>>().Value);

        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(config.CultureInfoName);

        return config;
    }

    private static void AddEsceptionFilter(MvcOptions options) => options.Filters.Add<GlobalExceptionFilter>();

    private static void AddBotTo(IServiceCollection services)
    {
        services.AddSingleton<BotSingleton>();
        services.AddHostedService<BotService>();
    }

    private static void AddCalendarTo(IServiceCollection services, Config config)
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