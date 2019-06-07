using BotApp.Extensions.Common.Translator.Domain;
using BotApp.Extensions.Common.Translator.Services;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BotApp.Extensions.Tests
{
    public class TranslatorServiceTest : IDisposable
    {
        private string EnvironmentName { get; set; } = nameof(TranslatorServiceTest);
        private string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        private TranslatorConfig configuration = new TranslatorConfig()
        {
            TranslatorTextAPIKey = "translator_text_api_key"
        };

        public TranslatorServiceTest()
        {
            dynamic dynamicConfiguration = new ExpandoObject();
            dynamicConfiguration.TranslatorConfig = configuration;
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
            ITranslatorService translatorService = new TranslatorService(httpClient, EnvironmentName, ContentRootPath);
            TranslatorConfig config = translatorService.GetConfiguration();

            // assert
            Assert.Equal(configuration.TranslatorTextAPIKey, config.TranslatorTextAPIKey);
        }

        [Fact]
        public async void GetDesiredLanguageTest()
        {
            // arrage
            var expectedLanguage = "es";
            var desiredLanguageResponse = new List<DesiredLanguageResponse>() { new DesiredLanguageResponse() { language = expectedLanguage, alternatives = new List<DesiredLanguageResponse>() { new DesiredLanguageResponse() } } };
            var jsonDesiredLanguageResponse = JsonConvert.SerializeObject(desiredLanguageResponse);
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
                   Content = new StringContent(jsonDesiredLanguageResponse),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };

            // act
            ITranslatorService translatorService = new TranslatorService(httpClient, EnvironmentName, ContentRootPath);
            var language = await translatorService.GetDesiredLanguageAsync("hola");

            // assert
            Assert.Equal(expectedLanguage, language);
        }

        [Fact]
        public async void TranslateSentenceTest()
        {
            // arrage
            var expectedTranslation = "hello";
            List<Dictionary<string, List<Dictionary<string, string>>>> translationResponse = new List<Dictionary<string, List<Dictionary<string, string>>>>();
            Dictionary<string, List<Dictionary<string, string>>> translationResponse_Dictionary = new Dictionary<string, List<Dictionary<string, string>>>();
            List<Dictionary<string, string>> translationResponse_Dictionary_List = new List<Dictionary<string, string>>();
            Dictionary<string, string> translationResponse_Dictionary_List_Dictionary = new Dictionary<string, string>();

            translationResponse_Dictionary_List_Dictionary.Add("text", "hello");

            translationResponse_Dictionary_List.Add(translationResponse_Dictionary_List_Dictionary);
            translationResponse_Dictionary.Add("translations", translationResponse_Dictionary_List);
            translationResponse.Add(translationResponse_Dictionary);

            var jsonTranslationResponse = JsonConvert.SerializeObject(translationResponse);
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
                   Content = new StringContent(jsonTranslationResponse),
               })
               .Verifiable();

            var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };

            // act
            ITranslatorService translatorService = new TranslatorService(httpClient, EnvironmentName, ContentRootPath);
            var translation = await translatorService.TranslateSentenceAsync("hola", "es", "en");

            // assert
            Assert.Equal(expectedTranslation, translation);
        }
    }
}