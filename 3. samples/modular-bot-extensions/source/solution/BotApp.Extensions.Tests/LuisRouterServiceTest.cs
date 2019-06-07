using BotApp.Extensions.BotBuilder.LuisRouter.Domain;
using BotApp.Extensions.BotBuilder.LuisRouter.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BotApp.Extensions.Tests
{
    public class LuisRouterServiceTest : IDisposable
    {
        private string EnvironmentName { get; set; } = nameof(LuisRouterServiceTest);
        private string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        private LuisRouterConfig configuration = new LuisRouterConfig()
        {
            BingSpellCheckSubscriptionKey = Guid.NewGuid().ToString(),
            LuisApplications = new List<LuisApp>() { new LuisApp() { AppId = Guid.NewGuid().ToString(), AuthoringKey = Guid.NewGuid().ToString(), Endpoint = "http://endpoint", Name = "name" } },
            LuisRouterUrl = "http://luis_router_url"
        };

        public LuisRouterServiceTest()
        {
            dynamic dynamicConfiguration = new ExpandoObject();
            dynamicConfiguration.LuisRouterConfig = configuration;
            var jsonConfiguration = JsonConvert.SerializeObject(dynamicConfiguration);
            File.WriteAllText(Path.Combine(ContentRootPath, $"appsettings.{EnvironmentName}.json"), jsonConfiguration);
        }

        public void Dispose()
        {
            File.Delete(Path.Combine(ContentRootPath, $"appsettings.{EnvironmentName}.json"));
        }

        [Fact]
        public async void GetConfigurationTest()
        {
            // arrage
            var httpClient = new HttpClient();
            var storage = new MemoryStorage();
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);
            var adapter = new TestAdapter().Use(new AutoSaveStateMiddleware(conversationState));
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            var steps = new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    await step.Context.SendActivityAsync("response");

                    // act
                    ILuisRouterService luisRouterService = new LuisRouterService(httpClient, EnvironmentName, ContentRootPath, userState);
                    LuisRouterConfig config = luisRouterService.GetConfiguration();

                    // assert
                    Assert.Equal(configuration.BingSpellCheckSubscriptionKey, config.BingSpellCheckSubscriptionKey);
                    Assert.Collection<LuisApp>(configuration.LuisApplications, x=> Xunit.Assert.Contains("name", x.Name));
                    Assert.Equal(configuration.LuisRouterUrl, config.LuisRouterUrl);

                    return Dialog.EndOfTurn;
                }
            };
            dialogs.Add(new WaterfallDialog(
                "test",
                steps));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("test", null, cancellationToken);
                }
            })
            .Send("ask")
            .AssertReply("response")
            .StartTestAsync();
        }

        [Fact]
        public async void GetTokenTest()
        {
            // arrage
            var expectedToken = "TOKEN";
            var identityResponse = new IdentityResponse() { token = expectedToken };
            var jsonIdentityResponse = JsonConvert.SerializeObject(identityResponse);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(jsonIdentityResponse),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };
            var storage = new MemoryStorage();
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);
            var adapter = new TestAdapter().Use(new AutoSaveStateMiddleware(conversationState));
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            var steps = new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    await step.Context.SendActivityAsync("response");

                    // act
                    ILuisRouterService luisRouterService = new LuisRouterService(httpClient, EnvironmentName, ContentRootPath, userState);
                    await luisRouterService.GetTokenAsync(step, "encrypted");

                    string token = await luisRouterService.TokenPreference.GetAsync(step.Context, () => { return string.Empty; });

                    // assert
                    Assert.Equal(expectedToken, token);

                    return Dialog.EndOfTurn;
                }
            };
            dialogs.Add(new WaterfallDialog(
                "test",
                steps));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("test", null, cancellationToken);
                }
            })
            .Send("ask")
            .AssertReply("response")
            .StartTestAsync();
        }

        [Fact]
        public async void LuisDiscoveryTest()
        {
            // arrage
            var expectedIntent = "Sample";
            var expectedName = "Sample";
            var expectedScore = 100;
            var luisAppDetail = new LuisAppDetail() { Intent = expectedIntent, Name = expectedName, Score = expectedScore };
            var luisDiscoveryResponse = new LuisDiscoveryResponse()
            {
                IsSucceded = true,
                ResultId = 100,
                LuisAppDetails = new List<LuisAppDetail>() { luisAppDetail }
            };
            var jsonLuisDiscoveryResponse = JsonConvert.SerializeObject(luisDiscoveryResponse);

            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync(new HttpResponseMessage()
               {
                   StatusCode = HttpStatusCode.OK,
                   Content = new StringContent(jsonLuisDiscoveryResponse),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };
            var storage = new MemoryStorage();
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);
            var adapter = new TestAdapter().Use(new AutoSaveStateMiddleware(conversationState));
            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            var steps = new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    await step.Context.SendActivityAsync("response");

                    // act
                     ILuisRouterService luisRouterService = new LuisRouterService(httpClient, EnvironmentName, ContentRootPath, userState);
                    var result = await luisRouterService.LuisDiscoveryAsync(step, "TEXT", "APPLICATIONCODE", "ENCRYPTIONKEY");
                    var item = result.ToList().FirstOrDefault();

                    // assert
                    Assert.Equal(expectedIntent, item.Intent);
                    Assert.Equal(expectedName, item.Name);
                    Assert.Equal(expectedScore, item.Score);

                    return Dialog.EndOfTurn;
                }
            };
            dialogs.Add(new WaterfallDialog(
                "test",
                steps));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);
                await dc.ContinueDialogAsync(cancellationToken);
                if (!turnContext.Responded)
                {
                    await dc.BeginDialogAsync("test", null, cancellationToken);
                }
            })
            .Send("ask")
            .AssertReply("response")
            .StartTestAsync();
        }
    }
}