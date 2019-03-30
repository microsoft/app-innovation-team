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
            // Retrieve configuration from sections
            Settings.MicrosoftAppId = Configuration.GetSection("MicrosoftAppId")?.Value;
            Settings.MicrosoftAppPassword = Configuration.GetSection("MicrosoftAppPassword")?.Value;
            Settings.TranslatorTextAPIKey = Configuration.GetSection("TranslatorTextAPIKey")?.Value;
            Settings.BotVersion = Configuration.GetSection("BotVersion")?.Value;
            Settings.LuisAppId01 = Configuration.GetSection("LuisAppId01")?.Value;
            Settings.LuisName01 = Configuration.GetSection("LuisName01")?.Value;
            Settings.LuisAuthoringKey01 = Configuration.GetSection("LuisAuthoringKey01")?.Value;
            Settings.LuisEndpoint01 = Configuration.GetSection("LuisEndpoint01")?.Value;

            IStorage storage = new MemoryStorage();

            services.AddBot<Bot>(options =>
            {
                options.State.Add(new UserState(storage));
                options.State.Add(new ConversationState(storage));

                options.CredentialProvider = new SimpleCredentialProvider(Settings.MicrosoftAppId, Settings.MicrosoftAppPassword);

                // The BotStateSet middleware forces state storage to auto-save when the bot is complete processing the message.
                // Note: Developers may choose not to add all the state providers to this middleware if save is not required.
                options.Middleware.Add(new AutoSaveStateMiddleware(options.State.ToArray()));
                options.Middleware.Add(new ShowTypingMiddleware());
            });

            services.AddSingleton(sp =>
            {
                // We need to grab the conversationState we added on the options in the previous step
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                if (options == null)
                {
                    throw new InvalidOperationException("BotFrameworkOptions must be configured prior to setting up the State Accessors");
                }

                var conversationState = options.State.OfType<ConversationState>().FirstOrDefault();
                if (conversationState == null)
                {
                    throw new InvalidOperationException("ConversationState must be defined and added before adding conversation-scoped state accessors.");
                }

                var userState = options.State.OfType<UserState>().FirstOrDefault();
                if (userState == null)
                {
                    throw new InvalidOperationException("UserState must be defined and added before adding user-scoped state accessors.");
                }

                var luisServices = new Dictionary<string, LuisRecognizer>();
                var app = new LuisApplication(Settings.LuisAppId01, Settings.LuisAuthoringKey01, Settings.LuisEndpoint01);
                var recognizer = new LuisRecognizer(app);
                luisServices.Add(Settings.LuisName01, recognizer);

                // Create the custom state accessor.
                // State accessors enable other components to read and write individual properties of state.
                var accessors = new BotAccessors(conversationState, userState, luisServices)
                {
                    ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                    LanguagePreference = userState.CreateProperty<string>("LanguagePreference"),
                    IsReadyForLUISPreference = userState.CreateProperty<bool>("IsReadyForLUISPreference")
                };

                return accessors;
            });
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