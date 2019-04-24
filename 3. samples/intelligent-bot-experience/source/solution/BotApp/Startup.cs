using Consul;
using FaceClientSDK;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.BotFramework;
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
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Reading settings
            Settings.MicrosoftAppId = Configuration.GetSection("MicrosoftAppId")?.Value;
            Settings.MicrosoftAppPassword = Configuration.GetSection("MicrosoftAppPassword")?.Value;

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

            Settings.AzureWebJobsStorage = Configuration.GetSection("AzureWebJobsStorage")?.Value;
            Settings.FaceAPIKey = Configuration.GetSection("FaceAPIKey")?.Value;
            Settings.FaceAPIZone = Configuration.GetSection("FaceAPIZone")?.Value;
            Settings.LargeFaceListId = Configuration.GetSection("LargeFaceListId")?.Value;
            Settings.MongoDBConnectionString = Configuration.GetSection("MongoDBConnectionString")?.Value;
            Settings.MongoDBDatabaseId = Configuration.GetSection("MongoDBDatabaseId")?.Value;
            Settings.PersonCollection = Configuration.GetSection("PersonCollection")?.Value;

            APIReference.FaceAPIKey = Settings.FaceAPIKey;
            APIReference.FaceAPIZone = Settings.FaceAPIZone;

            // Adding storage
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

            // Adding communication to discovery service
            services.AddSingleton<Microsoft.Extensions.Hosting.IHostedService, ConsulHostedService>();
            services.Configure<ConsulConfig>(Configuration.GetSection("ConsulConfig"));
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = Configuration["ConsulConfig:address"];
                consulConfig.Address = new Uri(address);
            }));

            // Adding MVC compatibility
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Adding the credential provider to be used with the Bot Framework Adapter
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();

            // Adding the channel provider to be used with the Bot Framework Adapter
            services.AddSingleton<IChannelProvider, ConfigurationChannelProvider>();

            // Adding the Bot Framework Adapter with error handling enabled
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Adding middlewares
            services.AddSingleton(new AutoSaveStateMiddleware(userState, conversationState));
            services.AddSingleton(new ShowTypingMiddleware());

            // Adding accessors
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
                    AskForExamplePreference = conversationState.CreateProperty<bool>("AskForExamplePreference"),
                    DetectedFaceIdPreference = conversationState.CreateProperty<string>("DetectedFaceIdPreference"),
                    ImageUriPreference = conversationState.CreateProperty<string>("ImageUriPreference"),
                    HashPreference = conversationState.CreateProperty<string>("HashPreference"),
                    IsNewPreference = conversationState.CreateProperty<bool>("IsNewPreference"),
                    FullnamePreference = userState.CreateProperty<string>("FullnamePreference"),
                    NamePreference = userState.CreateProperty<string>("NamePreference"),
                    LastnamePreference = userState.CreateProperty<string>("LastnamePreference"),
                    IsAuthenticatedPreference = userState.CreateProperty<bool>("IsAuthenticatedPreference")
                };

                return accessors;
            });

            // Adding Dialog that will be run by the bot
            //services.AddSingleton<MainDialog>();

            // Adding the bot as a transient. In this case the ASP Controller is expecting an IBot
            services.AddTransient<IBot, BotAppBot>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory log)
        {
            loggerFactory = log;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseMvc();
        }

        private async void OnApplicationStopping()
        {
            ConsulClient client = new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(Configuration["ConsulConfig:address"]);
            });

            await client.Agent.ServiceDeregister(ConsulHostedService.RegistrationID);
        }
    }
}