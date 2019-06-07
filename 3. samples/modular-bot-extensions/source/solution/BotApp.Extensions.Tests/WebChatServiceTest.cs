using BotApp.Extensions.BotBuilder.Channel.WebChat.Domain;
using BotApp.Extensions.BotBuilder.Channel.WebChat.Services;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BotApp.Extensions.Tests
{
    public class WebChatServiceTest : IDisposable
    {
        private string EnvironmentName { get; set; } = nameof(WebChatServiceTest);
        private string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        private WebChatConfig configuration = new WebChatConfig()
        {
            Secret = "secret"
        };

        public WebChatServiceTest()
        {
            dynamic dynamicConfiguration = new ExpandoObject();
            dynamicConfiguration.WebChatConfig = configuration;
            var jsonConfiguration = JsonConvert.SerializeObject(dynamicConfiguration);
            File.WriteAllText(Path.Combine(ContentRootPath, $"appsettings.{EnvironmentName}.json"), jsonConfiguration);
        }

        public void Dispose()
        {
            File.Delete(Path.Combine(ContentRootPath, $"appsettings.{EnvironmentName}.json"));
        }

        [Fact]
        public void GetConfigurationTest()
        {
            // arrage
            var httpClient = new HttpClient();

            // act
            IWebChatService webChatService = new WebChatService(httpClient, EnvironmentName, ContentRootPath);
            WebChatConfig config = webChatService.GetConfiguration();

            // assert
            Assert.Equal(configuration.Secret, config.Secret);
        }

        [Fact]
        public async void GetDirectLineTokenTest()
        {
            // arrage
            var expectedGenerateResponse = new GenerateResponse() { conversationId = "conversation_id", expires_in = 100, token = "token" };
            var jsonExpectedGenerateResponse = JsonConvert.SerializeObject(expectedGenerateResponse);

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
                   Content = new StringContent(jsonExpectedGenerateResponse),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };

            // act
            IWebChatService webChatService = new WebChatService(httpClient, EnvironmentName, ContentRootPath);
            GenerateResponse response = await webChatService.GetDirectLineTokenAsync(configuration.Secret);

            // assert
            Assert.Equal(expectedGenerateResponse.conversationId, response.conversationId);
            Assert.Equal(expectedGenerateResponse.expires_in, response.expires_in);
            Assert.Equal(expectedGenerateResponse.token, response.token);
        }
    }
}