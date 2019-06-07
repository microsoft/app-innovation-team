using BotApp.Extensions.BotBuilder.QnAMaker.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace BotApp.Extensions.BotBuilder.QnAMaker.Services
{
    public class QnAMakerService : IQnAMakerService
    {
        private readonly QnAMakerConfig config = null;
        private readonly HttpClient httpClient = null;
        private readonly IBotTelemetryClient botTelemetryClient = null;

        public Dictionary<string, Microsoft.Bot.Builder.AI.QnA.QnAMaker> QnAMakerServices { get; }

        public QnAMakerService(HttpClient httpClient, string environmentName, string contentRootPath, IBotTelemetryClient botTelemetryClient = null)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(contentRootPath)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
              .AddEnvironmentVariables();

            var configuration = builder.Build();

            config = new QnAMakerConfig();
            configuration.GetSection("QnAMakerConfig").Bind(config);

            if (string.IsNullOrEmpty(config.Name))
                throw new ArgumentException("Missing value in QnAMakerConfig -> Name");

            if (string.IsNullOrEmpty(config.KbId))
                throw new ArgumentException("Missing value in QnAMakerConfig -> KbId");

            if (string.IsNullOrEmpty(config.Hostname))
                throw new ArgumentException("Missing value in QnAMakerConfig -> Hostname");

            if (string.IsNullOrEmpty(config.EndpointKey))
                throw new ArgumentException("Missing value in QnAMakerConfig -> EndpointKey");

            this.httpClient = httpClient ?? throw new ArgumentException("Missing value in HttpClient");
            this.botTelemetryClient = botTelemetryClient;
            this.QnAMakerServices = BuildDictionary();
        }

        public QnAMakerConfig GetConfiguration() => config;

        private Dictionary<string, Microsoft.Bot.Builder.AI.QnA.QnAMaker> BuildDictionary()
        {
            Dictionary<string, Microsoft.Bot.Builder.AI.QnA.QnAMaker> result = new Dictionary<string, Microsoft.Bot.Builder.AI.QnA.QnAMaker>();

            var qnaEndpoint = new Microsoft.Bot.Builder.AI.QnA.QnAMakerEndpoint()
            {
                KnowledgeBaseId = config.KbId,
                EndpointKey = config.EndpointKey,
                Host = config.Hostname,
            };

            var qnaOptions = new Microsoft.Bot.Builder.AI.QnA.QnAMakerOptions
            {
                ScoreThreshold = 0.3F
            };

            Microsoft.Bot.Builder.AI.QnA.QnAMaker qnaMaker = null;

            if (botTelemetryClient != null)
            {
                qnaMaker = new Microsoft.Bot.Builder.AI.QnA.QnAMaker(qnaEndpoint, qnaOptions, httpClient, botTelemetryClient, true);
            }
            else
            {
                qnaMaker = new Microsoft.Bot.Builder.AI.QnA.QnAMaker(qnaEndpoint, qnaOptions, httpClient);
            }

            result.Add(config.Name, qnaMaker);

            return result;
        }
    }
}