using BotApp.Extensions.BotBuilder.Channel.WebChat.Services;
using BotApp.Extensions.Tests.Fakes;
using Xunit;

namespace BotApp.Extensions.Tests
{
    public class WebChatServiceTests
    {
        [Fact]
        public void GetConfigurationTest()
        {
            // arrange
            IWebChatService fakeWebChatService = new FakeWebChatService();
            var expectedSecret = "secret";

            // act
            var result = fakeWebChatService.GetConfiguration();

            // assert
            Assert.Equal(expectedSecret, result.Secret);
        }
    }
}