using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BotApp
{
    public class Startup
    {
        private ILoggerFactory loggerFactory;

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
            Settings.BotVersion = Configuration.GetSection("BotVersion")?.Value;
            Settings.TimeZone = Configuration.GetSection("TimeZone")?.Value;
            Settings.BotConversationStorageConnectionString = Configuration.GetSection("BotConversationStorageConnectionString")?.Value;
            Settings.BotConversationStorageKey = Configuration.GetSection("BotConversationStorageKey")?.Value;
            Settings.BotConversationStorageDatabaseId = Configuration.GetSection("BotConversationStorageDatabaseId")?.Value;
            Settings.BotConversationStorageUserCollection = Configuration.GetSection("BotConversationStorageUserCollection")?.Value;
            Settings.BotConversationStorageConversationCollection = Configuration.GetSection("BotConversationStorageConversationCollection")?.Value;
            Settings.LuisAppId01 = Configuration.GetSection("LuisAppId01")?.Value;
            Settings.LuisName01 = Configuration.GetSection("LuisName01")?.Value;
            Settings.LuisAuthoringKey01 = Configuration.GetSection("LuisAuthoringKey01")?.Value;
            Settings.LuisEndpoint01 = Configuration.GetSection("LuisEndpoint01")?.Value;
            Settings.QnAKbId01 = Configuration.GetSection("QnAKbId01")?.Value;
            Settings.QnAName01 = Configuration.GetSection("QnAName01")?.Value;
            Settings.QnAEndpointKey01 = Configuration.GetSection("QnAEndpointKey01")?.Value;
            Settings.QnAHostname01 = Configuration.GetSection("QnAHostname01")?.Value;

            CosmosDbStorage userstorage = new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = Settings.BotConversationStorageKey,
                CollectionId = Settings.BotConversationStorageUserCollection,
                CosmosDBEndpoint = new Uri(Settings.BotConversationStorageConnectionString),
                DatabaseId = Settings.BotConversationStorageDatabaseId,
            });

            CosmosDbStorage conversationstorage = new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = Settings.BotConversationStorageKey,
                CollectionId = Settings.BotConversationStorageConversationCollection,
                CosmosDBEndpoint = new Uri(Settings.BotConversationStorageConnectionString),
                DatabaseId = Settings.BotConversationStorageDatabaseId,
            });

            var userState = new UserState(userstorage);
            var conversationState = new ConversationState(conversationstorage);

            services.AddSingleton(userState);
            services.AddSingleton(conversationState);

            services.AddBot<Bot>(options =>
            {
                options.CredentialProvider = new SimpleCredentialProvider(Settings.MicrosoftAppId, Settings.MicrosoftAppPassword);

                // The BotStateSet middleware forces state storage to auto-save when the bot is complete processing the message.
                // Note: Developers may choose not to add all the state providers to this middleware if save is not required.
                options.Middleware.Add(new AutoSaveStateMiddleware(userState, conversationState));
                options.Middleware.Add(new ShowTypingMiddleware());

                // Creates a logger for the application to use.
                ILogger logger = loggerFactory.CreateLogger<Bot>();

                // Catches any errors that occur during a conversation turn and logs them.
                options.OnTurnError = async (context, exception) =>
                {
                    logger.LogError($"Exception caught : {exception}");
                    await context.SendActivityAsync("Sorry, it looks like something went wrong.");
                };
            });

            services.AddSingleton(sp =>
            {
                // We need to grab the conversationState we added on the options in the previous step
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                if (options == null)
                {
                    throw new InvalidOperationException("BotFrameworkOptions must be configured prior to setting up the State Accessors");
                }

                var luisServices = new Dictionary<string, LuisRecognizer>();
                var app = new LuisApplication(Settings.LuisAppId01, Settings.LuisAuthoringKey01, Settings.LuisEndpoint01);
                var recognizer = new LuisRecognizer(app);
                luisServices.Add(Settings.LuisName01, recognizer);

                var qnaEndpoint = new QnAMakerEndpoint()
                {
                    KnowledgeBaseId = Settings.QnAKbId01,
                    EndpointKey = Settings.QnAEndpointKey01,
                    Host = Settings.QnAHostname01,
                };

                var qnaOptions = new QnAMakerOptions
                {
                    ScoreThreshold = 0.3F
                };
                
                var qnaServices = new Dictionary<string, QnAMaker>();
                var qnaMaker = new QnAMaker(qnaEndpoint, qnaOptions);
                qnaServices.Add(Settings.QnAName01, qnaMaker);

                // Create the custom state accessor.
                // State accessors enable other components to read and write individual properties of state.
                var accessors = new BotAccessors(loggerFactory, conversationState, userState, luisServices, qnaServices)
                {
                    ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                    AskForExamplePreference = conversationState.CreateProperty<bool>("AskForExamplePreference")
                };

                return accessors;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory log)
        {
            loggerFactory = log;

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