using BotApp.Extensions.BotBuilder.LuisRouter.Domain;
using BotApp.Extensions.BotBuilder.LuisRouter.Services;
using BotApp.Extensions.Tests.Fakes;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BotApp.Extensions.Tests
{
    public class LuisRouterServiceTests
    {
        [Fact]
        public void GetConfigurationTest()
        {
            // arrage
            ILuisRouterService fakeWebChatService = new FakeLuisRouterService();

            var expectedBingSpellCheckSubscriptionKey = "bing_spell_check_subscription_key";
            var expectedEnableLuisTelemetry = true;
            var expectedLuisApplications = new List<LuisApp>();
            var expectedLuisRouterUrl = "luis_router_url";

            // act
            var result = fakeWebChatService.GetConfiguration();

            // assert
            Assert.Equal(expectedBingSpellCheckSubscriptionKey, result.BingSpellCheckSubscriptionKey);
            Assert.Equal(expectedEnableLuisTelemetry, result.EnableLuisTelemetry);
            Assert.Equal(expectedLuisApplications, result.LuisApplications);
            Assert.Equal(expectedLuisRouterUrl, result.LuisRouterUrl);
        }

        [Fact]
        public async void GetTokenTest()
        {
            // arrange
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

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost/"),
            };

            // bot context
            var storage = new MemoryStorage();
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            var steps = new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    await step.Context.SendActivityAsync("step1");

                    // act
                    ILuisRouterService fakeLuisRouterService = new FakeLuisRouterService(httpClient, userState);
                    await fakeLuisRouterService.GetTokenAsync(step, "encrypted");

                    string token = await fakeLuisRouterService.TokenPreference.GetAsync(step.Context, () => { return string.Empty; });

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
            .Send("hello")
            .AssertReply("step1")
            .StartTestAsync();
        }

        [Fact]
        public async void LuisDiscoveryTest()
        {
            // arrange
            var expectedIntent = "Sample";
            var expectedName = "Sample";
            var expectedScore = 100;
            var luisAppDetail = new LuisAppDetail() { Intent = expectedIntent, Name = expectedName, Score = expectedScore };
            var luisDiscoveryResponseResult = new LuisDiscoveryResponseResult()
            {
                Result = new LuisDiscoveryResponse()
                {
                    IsSucceded = true,
                    ResultId = 100,
                    LuisAppDetails = new List<LuisAppDetail>() { luisAppDetail }
                }
            };
            var jsonLuisDiscoveryResponseResult = JsonConvert.SerializeObject(luisDiscoveryResponseResult);

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
                   Content = new StringContent(jsonLuisDiscoveryResponseResult),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost/"),
            };

            // bot context
            var storage = new MemoryStorage();
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            var dialogState = conversationState.CreateProperty<DialogState>("dialogState");
            var dialogs = new DialogSet(dialogState);
            var steps = new WaterfallStep[]
            {
                async (step, cancellationToken) =>
                {
                    await step.Context.SendActivityAsync("step1");

                    // act
                    ILuisRouterService fakeLuisRouterService = new FakeLuisRouterService(httpClient, userState);
                    var result = await fakeLuisRouterService.LuisDiscoveryAsync(step, "TEXT", "APPLICATIONCODE", "ENCRYPTIONKEY");
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
            .Send("hello")
            .AssertReply("step1")
            .StartTestAsync();
        }
    }
}