using BotApp.Extensions.BotBuilder.ActiveDirectory.Services;
using BotApp.Extensions.Tests.Fakes;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Newtonsoft.Json.Linq;
using Xunit;

namespace BotApp.Extensions.Tests
{
    public class ActiveDirectoryServiceTests
    {
        [Fact]
        public void GetConfigurationTest()
        {
            // arrange
            var expectedValidAudience = "valid_audience";
            var expectedValidIssuer = "valid_issuer";

            // act
            IActiveDirectoryService fakeActiveDirectoryService = new FakeActiveDirectoryService();
            var result = fakeActiveDirectoryService.GetConfiguration();

            // assert
            Assert.Equal(expectedValidAudience, result.ValidAudience);
            Assert.Equal(expectedValidIssuer, result.ValidIssuer);
        }

        [Fact]
        public async void ValidateValidTokenTest()
        {
            // arrange
            var token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJNb2R1bGFyIEJvdCBBcHAiLCJpYXQiOjE1NTk2NzQ1OTUsImV4cCI6MTU5MTIxMDU5NSwiYXVkIjoiQm90IEFwcCBBdWRpZW5jZSIsInN1YiI6IkJvdCBBcHAiLCJHaXZlbk5hbWUiOiJKb2hubnkiLCJTdXJuYW1lIjoiUm9ja2V0IiwiRW1haWwiOiJqcm9ja2V0QGV4YW1wbGUuY29tIiwiUm9sZSI6WyJNYW5hZ2VyIiwiUHJvamVjdCBBZG1pbmlzdHJhdG9yIl19.5UFBWMnBFgz0GHRzynDv8eyYuiv9doy_bFcno5sNQe4";
            var expectedResult = true;

            var storage = new MemoryStorage();
            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            ITurnContext turnContext = new TurnContext(adapter, new Microsoft.Bot.Schema.Activity());
            dynamic jsonObject = new JObject();
            jsonObject.token = token;
            turnContext.Activity.ChannelData = jsonObject;

            // act
            IActiveDirectoryService fakeActiveDirectoryService = new FakeActiveDirectoryService();
            var result = await fakeActiveDirectoryService.ValidateTokenAsync(turnContext);

            // assert
            Assert.Equal(expectedResult, result);
        }
    }
}