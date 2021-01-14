﻿using System.Diagnostics.CodeAnalysis;
using GryphonUtilityBot.Web.Models;
using GryphonUtilityBot.Web.Models.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GryphonUtilityBot.Web
{
    internal sealed class Startup
    {
        public Startup(IConfiguration config) => _config = config;

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IBot, Bot>();
            services.AddHostedService<Service>();
            services.Configure<Config>(_config);

            services.AddMvc();
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMvc(routes => routes.MapRoute("update", $"{_config["Token"]}/{{controller=Update}}/{{action=post}}"));
        }

        private readonly IConfiguration _config;
    }
}