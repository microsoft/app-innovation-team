using BotApp.Extensions.BotBuilder.LuisRouter.Accessors;
using BotApp.Extensions.BotBuilder.LuisRouter.Helpers;
using BotApp.Extensions.BotBuilder.QnAMaker.Accessors;
using BotApp.Extensions.BotBuilder.QnAMaker.Helpers;
using BotApp.Extensions.Common.Consul.Helpers;
using BotApp.Extensions.Common.KeyVault.Helpers;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.ApplicationInsights;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.ApplicationInsights.Core;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace BotApp
{
    public class Startup
    {
        private ILoggerFactory loggerFactory;
        public static string EnvironmentName { get; set; }
        public static string ContentRootPath { get; set; }
        public static string EncryptionKey { get; set; }
        public static string ApplicationCode { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
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
            // Reading settings
            Settings.MicrosoftAppId = Configuration.GetSection("ApplicationSettings:MicrosoftAppId")?.Value;
            Settings.MicrosoftAppPassword = Configuration.GetSection("ApplicationSettings:MicrosoftAppPassword")?.Value;
            Settings.BotConversationStorageConnectionString = Configuration.GetSection("ApplicationSettings:BotConversationStorageConnectionString")?.Value;
            Settings.BotConversationStorageKey = Configuration.GetSection("ApplicationSettings:BotConversationStorageKey")?.Value;
            Settings.BotConversationStorageDatabaseId = Configuration.GetSection("ApplicationSettings:BotConversationStorageDatabaseId")?.Value;
            Settings.BotConversationStorageUserCollection = Configuration.GetSection("ApplicationSettings:BotConversationStorageUserCollection")?.Value;
            Settings.BotConversationStorageConversationCollection = Configuration.GetSection("ApplicationSettings:BotConversationStorageConversationCollection")?.Value;

            // Adding EncryptionKey and ApplicationCode
            using (KeyVaultHelper keyVaultHelper = new KeyVaultHelper(EnvironmentName, ContentRootPath))
            {
                Settings.KeyVaultEncryptionKey = Configuration.GetSection("ApplicationSettings:KeyVaultEncryptionKey")?.Value;
                EncryptionKey = keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultEncryptionKey).Result;

                Settings.KeyVaultApplicationCode = Configuration.GetSection("ApplicationSettings:KeyVaultApplicationCode")?.Value;
                ApplicationCode = keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultApplicationCode).Result;
            }

            // Adding CosmosDB user storage
            CosmosDbStorage userstorage = new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = Settings.BotConversationStorageKey,
                CollectionId = Settings.BotConversationStorageUserCollection,
                CosmosDBEndpoint = new Uri(Settings.BotConversationStorageConnectionString),
                DatabaseId = Settings.BotConversationStorageDatabaseId,
            });

            var userState = new UserState(userstorage);
            services.AddSingleton(userState);

            // Adding CosmosDB conversation storage
            CosmosDbStorage conversationstorage = new CosmosDbStorage(new CosmosDbStorageOptions
            {
                AuthKey = Settings.BotConversationStorageKey,
                CollectionId = Settings.BotConversationStorageConversationCollection,
                CosmosDBEndpoint = new Uri(Settings.BotConversationStorageConnectionString),
                DatabaseId = Settings.BotConversationStorageDatabaseId,
            });

            var conversationState = new ConversationState(conversationstorage);
            services.AddSingleton(conversationState);

            // Adding Consul hosted service
            using (ConsulHelper consulHelper = new ConsulHelper(EnvironmentName, ContentRootPath))
            {
                consulHelper.Initialize(services, Configuration);
            }

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

            // Adding LUIS Router accessor
            services.AddSingleton(sp =>
            {
                LuisRouterAccessor accessor = null;
                using (LuisRouterHelper luisRouterHelper = new LuisRouterHelper(EnvironmentName, ContentRootPath))
                {
                    accessor = luisRouterHelper.BuildAccessor(userState, sp.GetRequiredService<IBotTelemetryClient>());
                }
                return accessor;
            });

            // Adding QnAMaker Router accessor
            services.AddSingleton(sp =>
            {
                QnAMakerAccessor accessor = null;
                using (QnAMakerHelper qnaMakerHelper = new QnAMakerHelper(EnvironmentName, ContentRootPath))
                {
                    accessor = qnaMakerHelper.BuildAccessor();
                }
                return accessor;
            });

            // Adding accessors
            services.AddSingleton(sp =>
            {
                // We need to grab the conversationState we added on the options in the previous step
                var options = sp.GetRequiredService<IOptions<BotFrameworkOptions>>().Value;
                if (options == null)
                {
                    throw new InvalidOperationException("BotFrameworkOptions must be configured prior to setting up the State Accessors");
                }

                // Create the custom state accessor.
                // State accessors enable other components to read and write individual properties of state.
                var accessors = new BotAccessors(loggerFactory, conversationState, userState)
                {
                    ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                    AskForExamplePreference = conversationState.CreateProperty<bool>("AskForExamplePreference"),
                    IsAuthenticatedPreference = userState.CreateProperty<bool>("IsAuthenticatedPreference")
                };

                return accessors;
            });

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
                .UseBotApplicationInsights()
                .UseMvc();
        }

        private async Task OnApplicationStopping()
        {
            using (ConsulHelper consulHelper = new ConsulHelper(EnvironmentName, ContentRootPath))
            {
                await consulHelper.Stop();
            }
        }
    }
}