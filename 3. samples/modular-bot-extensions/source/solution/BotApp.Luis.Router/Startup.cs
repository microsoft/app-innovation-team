using BotApp.Extensions.Common.Consul.Helpers;
using BotApp.Luis.Router.Domain;
using BotApp.Luis.Router.Domain.Settings;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using System.Text;

namespace BotApp.Luis.Router
{
    public class Startup
    {
        public static string EnvironmentName { get; set; }
        public static string ContentRootPath { get; set; }

        public Startup(IHostingEnvironment env)
        {
            // Specify the environment name
            EnvironmentName = env.EnvironmentName;

            // Specify the content root path
            ContentRootPath = env.ContentRootPath;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            Settings.AuthorizationKey = Configuration.GetSection("ApplicationSettings:AuthorizationKey")?.Value;

            // Add Application Insights services into service collection
            services.AddApplicationInsightsTelemetry();

            // Add the standard telemetry client
            services.AddSingleton<IBotTelemetryClient, BotTelemetryClient>();

            // Add ASP middleware to store the HTTP body, mapped with bot activity key, in the httpcontext.items
            // This will be picked by the TelemetryBotIdInitializer
            services.AddTransient<TelemetrySaveBodyASPMiddleware>();

            // Add telemetry initializer that will set the correlation context for all telemetry items
            services.AddSingleton<ITelemetryInitializer, OperationCorrelationTelemetryInitializer>();

            // Add telemetry initializer that sets the user ID and session ID (in addition to other
            // bot-specific properties, such as activity ID)
            services.AddSingleton<ITelemetryInitializer, TelemetryBotIdInitializer>();

            // Create the telemetry middleware to track conversation events
            services.AddSingleton<IMiddleware, TelemetryLoggerMiddleware>();

            var apps = new List<LuisAppRegistration>();
            Configuration.GetSection("LuisAppRegistrations").Bind(apps);
            Settings.LuisAppRegistrations = apps;

            // Adding Consul hosted service
            using (ConsulService consulService = new ConsulService(EnvironmentName, ContentRootPath))
            {
                consulService.Initialize(services, Configuration);
            }

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                 .AddJwtBearer(options =>
                 {
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidateAudience = true,
                         ValidateLifetime = true,
                         ValidateIssuerSigningKey = true,

                         ValidIssuer = "https://BotApp.Luis.Router.Identity",
                         ValidAudience = "https://BotApp.Luis.Router.Identity",
                         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.AuthorizationKey))
                     };
                 });

            services.AddCors(o => o.AddPolicy("AllowAllPolicy", options =>
            {
                options.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            applicationLifetime.ApplicationStopping.Register(OnApplicationStopping);

            app.UseDefaultFiles()
                .UseStaticFiles();

            app.UseCors("AllowAllPolicy");

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }

        private async void OnApplicationStopping()
        {
            using (ConsulService consulService = new ConsulService(EnvironmentName, ContentRootPath))
            {
                await consulService.Stop();
            }
        }
    }
}