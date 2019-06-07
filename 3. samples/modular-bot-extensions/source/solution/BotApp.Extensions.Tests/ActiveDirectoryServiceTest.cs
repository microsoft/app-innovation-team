using BotApp.Extensions.BotBuilder.ActiveDirectory.Domain;
using BotApp.Extensions.BotBuilder.ActiveDirectory.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.Dialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Dynamic;
using System.IO;
using Xunit;

namespace BotApp.Extensions.Tests
{
    public class ActiveDirectoryServiceTest : IDisposable
    {
        private string EnvironmentName { get; set; } = nameof(ActiveDirectoryServiceTest);
        private string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        private ActiveDirectoryConfig configuration = new ActiveDirectoryConfig()
        {
            ValidAudience = "valid_audience",
            ValidIssuer = "valid_issuer"
        };

        public ActiveDirectoryServiceTest()
        {
            dynamic dynamicConfiguration = new ExpandoObject();
            dynamicConfiguration.ActiveDirectoryConfig = configuration;
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
                    IActiveDirectoryService activeDirectoryService = new ActiveDirectoryService(EnvironmentName, ContentRootPath);
                    ActiveDirectoryConfig config = activeDirectoryService.GetConfiguration();

                    // assert
                    Assert.Equal(configuration.ValidAudience, config.ValidAudience);
                    Assert.Equal(configuration.ValidIssuer, config.ValidIssuer);

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
        public async void ValidateValidTokenTest()
        {
            // arrage
            var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJNb2R1bGFyIEJvdCBBcHAiLCJpYXQiOjE1NTk2NzQ1OTUsImV4cCI6MTU5MTIxMDU5NSwiYXVkIjoiQm90IEFwcCBBdWRpZW5jZSIsInN1YiI6IkJvdCBBcHAiLCJHaXZlbk5hbWUiOiJKb2hubnkiLCJTdXJuYW1lIjoiUm9ja2V0IiwiRW1haWwiOiJqcm9ja2V0QGV4YW1wbGUuY29tIiwiUm9sZSI6WyJNYW5hZ2VyIiwiUHJvamVjdCBBZG1pbmlzdHJhdG9yIl19.5UFBWMnBFgz0GHRzynDv8eyYuiv9doy_bFcno5sNQe4";
            var expectedResult = true;
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
                    IActiveDirectoryService activeDirectoryService = new ActiveDirectoryService(EnvironmentName, ContentRootPath);
                    bool result = await activeDirectoryService.ValidateTokenAsync(step.Context, "Bot App Audience", "Modular Bot App", false, "qwertyuiopasdfghjklzxcvbnm123456");

                    // assert
                    Assert.Equal(expectedResult, result);

                    return Dialog.EndOfTurn;
                }
            };
            dialogs.Add(new WaterfallDialog(
                "test",
                steps));

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                // modifying channel data with token
                dynamic jsonObject = new JObject();
                jsonObject.token = token;
                turnContext.Activity.ChannelData = jsonObject;

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