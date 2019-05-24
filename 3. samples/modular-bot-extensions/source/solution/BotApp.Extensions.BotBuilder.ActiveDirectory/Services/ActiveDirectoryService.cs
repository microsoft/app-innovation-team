using BotApp.Extensions.BotBuilder.ActiveDirectory.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;

namespace BotApp.Extensions.BotBuilder.ActiveDirectory.Services
{
    public class ActiveDirectoryService : IActiveDirectoryService
    {
        private readonly ActiveDirectoryConfig config = null;

        public ActiveDirectoryService(string environmentName, string contentRootPath)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(contentRootPath)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
              .AddEnvironmentVariables();

            var configuration = builder.Build();

            config = new ActiveDirectoryConfig();
            configuration.GetSection("ActiveDirectoryConfig").Bind(config);

            if (string.IsNullOrEmpty(config.Secret))
                throw new Exception("Missing value in ActiveDirectoryConfig -> Secret");
        }

        public ActiveDirectoryConfig GetConfiguration() => config;

        public async Task<bool> ValidateActiveDirectoryTokenAsync(ITurnContext turnContext)
        {
            bool hasPermissionToTalk = true;
            string token = string.Empty;

            if (turnContext.Activity.ChannelData == null)
            {
                hasPermissionToTalk = false;
            }
            else
            {
                try
                {
                    var channelObj = turnContext.Activity.ChannelData.ToString();
                    var channeldata = Newtonsoft.Json.Linq.JObject.Parse(channelObj);
                    token = channeldata["token"].ToString();

                    if (channeldata == null)
                    {
                        hasPermissionToTalk = false;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(token))
                        {
                            hasPermissionToTalk = false;
                        }
                    }
                }
                catch
                {
                    hasPermissionToTalk = false;
                }
            }

            return hasPermissionToTalk;
        }
    }
}