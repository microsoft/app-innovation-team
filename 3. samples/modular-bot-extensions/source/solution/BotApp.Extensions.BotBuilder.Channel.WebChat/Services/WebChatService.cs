using BotApp.Extensions.BotBuilder.Channel.WebChat.Domain;
using Microsoft.Extensions.Configuration;
using System;

namespace BotApp.Extensions.BotBuilder.Channel.WebChat.Services
{
    public class WebChatService : IWebChatService
    {
        private readonly WebChatConfig config = null;

        public WebChatService(string environmentName, string contentRootPath)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(contentRootPath)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
              .AddEnvironmentVariables();

            var configuration = builder.Build();

            config = new WebChatConfig();
            configuration.GetSection("WebChatConfig").Bind(config);

            if (string.IsNullOrEmpty(config.Secret))
                throw new Exception("Missing value in WebChatConfig -> Secret");
        }

        public WebChatConfig GetConfiguration() => config;
    }
}