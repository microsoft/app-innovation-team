using BotApp.Extensions.BotBuilder.Channel.WebChat.Services;
using BotApp.Extensions.Tests.Fakes;
using System.Net.Http;
using Xunit;

namespace BotApp.Extensions.Tests
{
    public class WebChatServiceTests
    {
        [Fact]
        public void GetConfigurationTest()
        {
            // arrange
            var expectedSecret = "secret";

            // act
            IWebChatService fakeWebChatService = new FakeWebChatService();
            var result = fakeWebChatService.GetConfiguration();

            // assert
            Assert.Equal(expectedSecret, result.Secret);
        }

        [Fact]
        public async void GetDirectLineTokenTest()
        {
            // arrange

            // act
            var httpClient = new HttpClient();
            IWebChatService fakeWebChatService = new FakeWebChatService(httpClient);
            var result = await fakeWebChatService.GetDirectLineTokenAsync("VTW1EZ-5QVE.GvgAKIvgD8duXEpwMTYEmIxyJd9gJd_yO23fjwx5xUU");

            // assert
            Assert.True(!string.IsNullOrEmpty(result.token));
        }

    }
}