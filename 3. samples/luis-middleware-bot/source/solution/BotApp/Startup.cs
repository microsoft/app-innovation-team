using Consul;
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

        public static string EncryptedKey { get; set; }

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
            Settings.MicrosoftAppId = Configuration.GetSection("ApplicationSettings:MicrosoftAppId")?.Value;
            Settings.MicrosoftAppPassword = Configuration.GetSection("ApplicationSettings:MicrosoftAppPassword")?.Value;
            Settings.BotConversationStorageConnectionString = Configuration.GetSection("ApplicationSettings:BotConversationStorageConnectionString")?.Value;
            Settings.BotConversationStorageKey = Configuration.GetSection("ApplicationSettings:BotConversationStorageKey")?.Value;
            Settings.BotConversationStorageDatabaseId = Configuration.GetSection("ApplicationSettings:BotConversationStorageDatabaseId")?.Value;
            Settings.BotConversationStorageUserCollection = Configuration.GetSection("ApplicationSettings:BotConversationStorageUserCollection")?.Value;
            Settings.BotConversationStorageConversationCollection = Configuration.GetSection("ApplicationSettings:BotConversationStorageConversationCollection")?.Value;
            Settings.QnAKbId = Configuration.GetSection("ApplicationSettings:QnAKbId")?.Value;
            Settings.QnAName = Configuration.GetSection("ApplicationSettings:QnAName")?.Value;
            Settings.QnAEndpointKey = Configuration.GetSection("ApplicationSettings:QnAEndpointKey")?.Value;
            Settings.QnAHostname = Configuration.GetSection("ApplicationSettings:QnAHostname")?.Value;
            Settings.LuisMiddlewareUrl = Configuration.GetSection("ApplicationSettings:LuisMiddlewareUrl")?.Value;
            Settings.KeyVaultCertificateName = Configuration.GetSection("ApplicationSettings:KeyVaultCertificateName")?.Value;
            Settings.KeyVaultClientId = Configuration.GetSection("ApplicationSettings:KeyVaultClientId")?.Value;
            Settings.KeyVaultClientSecret = Configuration.GetSection("ApplicationSettings:KeyVaultClientSecret")?.Value;
            Settings.KeyVaultIdentifier = Configuration.GetSection("ApplicationSettings:KeyVaultIdentifier")?.Value;

            try
            {
                var apps = new List<LuisAppRegistration>();
                Configuration.GetSection("LuisAppRegistrations").Bind(apps);
                Settings.LuisAppRegistrations = apps;
            }
            catch
            {
                throw new Exception("There was an exception loading LUIS apps");
            }

            KeyVaultConnectionInfo keyVaultConnectionInfo = new KeyVaultConnectionInfo()
            {
                CertificateName = Settings.KeyVaultCertificateName,
                ClientId = Settings.KeyVaultClientId,
                ClientSecret = Settings.KeyVaultClientSecret,
                KeyVaultIdentifier = Settings.KeyVaultIdentifier
            };

            using (KeyVaultHelper keyVaultHelper = new KeyVaultHelper(keyVaultConnectionInfo))
            {
                Settings.KeyVaultEncryptionKey = Configuration.GetSection("ApplicationSettings:KeyVaultEncryptionKey")?.Value;
                var encryptionkey = keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultEncryptionKey).Result;

                Settings.KeyVaultApplicationCode = Configuration.GetSection("ApplicationSettings:KeyVaultApplicationCode")?.Value;
                var appcode = keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultApplicationCode).Result;
                EncryptedKey = NETCore.Encrypt.EncryptProvider.AESEncrypt(appcode, encryptionkey);
            }

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
                var address = Configuration["ConsulConfig:Address"];
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
                foreach (LuisAppRegistration app in Settings.LuisAppRegistrations)
                {
                    var luis = new LuisApplication(app.LuisAppId, app.LuisAuthoringKey, app.LuisEndpoint);
                    var recognizer = new LuisRecognizer(luis);
                    luisServices.Add(app.LuisName, recognizer);
                }

                var qnaEndpoint = new QnAMakerEndpoint()
                {
                    KnowledgeBaseId = Settings.QnAKbId,
                    EndpointKey = Settings.QnAEndpointKey,
                    Host = Settings.QnAHostname,
                };

                var qnaOptions = new QnAMakerOptions
                {
                    ScoreThreshold = 0.3F
                };

                var qnaServices = new Dictionary<string, QnAMaker>();
                var qnaMaker = new QnAMaker(qnaEndpoint, qnaOptions);
                qnaServices.Add(Settings.QnAName, qnaMaker);

                // Create the custom state accessor.
                // State accessors enable other components to read and write individual properties of state.
                var accessors = new BotAccessors(loggerFactory, conversationState, userState, luisServices, qnaServices)
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
                .UseMvc();
        }

        private async void OnApplicationStopping()
        {
            ConsulClient client = new ConsulClient(consulConfig =>
            {
                consulConfig.Address = new Uri(Configuration["ConsulConfig:Address"]);
            });

            await client.Agent.ServiceDeregister(ConsulHostedService.RegistrationID);
        }
    }
}