using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WBD
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile("secrets/appsettings.secrets.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // TODO: ADD SETTINGS

            IStorage storage = new MemoryStorage();

            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            services.AddSingleton(userState);
            services.AddSingleton(conversationState);

            services.AddBot<Bot>(options =>
            {
                options.CredentialProvider = new SimpleCredentialProvider(Settings.MicrosoftAppId, Settings.MicrosoftAppPassword);

                // The BotStateSet middleware forces state storage to auto-save when the bot is complete processing the message.
                // Note: Developers may choose not to add all the state providers to this middleware if save is not required.
                options.Middleware.Add(new AutoSaveStateMiddleware(userState, conversationState));
                options.Middleware.Add(new ShowTypingMiddleware());
            });

            // TODO: ADD ACCESSOR CONFIGURATION
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }
}