using BotApp.Extensions.BotBuilder.QnAMaker.Accessors;
using BotApp.Extensions.BotBuilder.QnAMaker.Domain;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace BotApp.Extensions.BotBuilder.QnAMaker.Helpers
{
    public class QnAMakerHelper : BaseHelper
    {
        private readonly QnAMakerConfig config = null;

        public QnAMakerHelper(string environmentName, string contentRootPath)
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
                throw new Exception("Missing value in QnAMakerConfig -> Name");

            if (string.IsNullOrEmpty(config.KbId))
                throw new Exception("Missing value in QnAMakerConfig -> KbId");

            if (string.IsNullOrEmpty(config.Hostname))
                throw new Exception("Missing value in QnAMakerConfig -> Hostname");

            if (string.IsNullOrEmpty(config.EndpointKey))
                throw new Exception("Missing value in QnAMakerConfig -> EndpointKey");
        }

        public QnAMakerConfig GetConfiguration() => config;

        public QnAMakerAccessor BuildAccessor()
        {
            Dictionary<string, Microsoft.Bot.Builder.AI.QnA.QnAMaker> qnaServices = BuildDictionary();
            return new QnAMakerAccessor(qnaServices) { };
        }

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

            var qnaMaker = new Microsoft.Bot.Builder.AI.QnA.QnAMaker(qnaEndpoint, qnaOptions);
            result.Add(config.Name, qnaMaker);

            return result;
        }
    }
}