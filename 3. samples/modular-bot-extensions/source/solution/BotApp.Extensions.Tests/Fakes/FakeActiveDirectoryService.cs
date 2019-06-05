using BotApp.Extensions.BotBuilder.ActiveDirectory.Domain;
using BotApp.Extensions.BotBuilder.ActiveDirectory.Services;
using Microsoft.Bot.Builder;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BotApp.Extensions.Tests.Fakes
{
    public class FakeActiveDirectoryService : IActiveDirectoryService
    {
        public ActiveDirectoryConfig GetConfiguration()
        {
            var activeDirectoryConfig = new ActiveDirectoryConfig()
            {
                ValidAudience = "valid_audience",
                ValidIssuer = "valid_issuer"
            };
            return activeDirectoryConfig;
        }

        public async Task<bool> ValidateTokenAsync(ITurnContext turnContext)
        {
            bool result = true;
            string token = string.Empty;
            if (ValidateContent(turnContext))
            {
                try
                {
                    var channelObj = turnContext.Activity.ChannelData.ToString();
                    var channeldata = Newtonsoft.Json.Linq.JObject.Parse(channelObj);
                    token = channeldata["token"].ToString();
                    await TokenValidationAsync(token);
                }
                catch (SecurityTokenException ex)
                {
                    Console.WriteLine(ex.Message);
                    result = false;
                }
            }

            return result;
        }

        private bool ValidateContent(ITurnContext turnContext)
        {
            bool result = true;
            string token = string.Empty;

            if (turnContext.Activity.ChannelData == null)
            {
                result = false;
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
                        result = false;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(token))
                        {
                            result = false;
                        }
                    }
                }
                catch
                {
                    result = false;
                }
            }

            return result;
        }

        private async Task<JwtSecurityToken> TokenValidationAsync(string token)
        {
            string stsDiscoveryEndpoint = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";

            ConfigurationManager<OpenIdConnectConfiguration> configManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
            OpenIdConnectConfiguration config = await configManager.GetConfigurationAsync();

            var securityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.Default.GetBytes("qwertyuiopasdfghjklzxcvbnm123456"));

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = "Bot App Audience",
                ValidateIssuer = true,
                ValidIssuer = "Modular Bot App",
                //IssuerSigningKeys = config.SigningKeys,
                IssuerSigningKey = securityKey,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false
            };

            JwtSecurityTokenHandler tokendHandler = new JwtSecurityTokenHandler();

            SecurityToken jwt;
            IdentityModelEventSource.ShowPII = false;
            ClaimsPrincipal claimsPrincipal = tokendHandler.ValidateToken(token, validationParameters, out jwt);

            return jwt as JwtSecurityToken;
        }
    }
}