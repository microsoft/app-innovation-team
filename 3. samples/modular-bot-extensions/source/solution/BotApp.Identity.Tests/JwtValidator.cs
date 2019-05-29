using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BotApp.Identity.Tests
{
    public class JwtValidator
    {
        public async Task<bool> ValidateAsync(string token)
        {
            var validationSucceeded = true;
            try
            {
                await InternalValidationAsync(token);
            }
            catch (SecurityTokenException ex)
            {
                Console.WriteLine(ex.Message);
                validationSucceeded = false;
            }

            return validationSucceeded;
        }

        private async Task<JwtSecurityToken> InternalValidationAsync(string token)
        {
            string stsDiscoveryEndpoint = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";

            ConfigurationManager<OpenIdConnectConfiguration> configManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
            OpenIdConnectConfiguration config = await configManager.GetConfigurationAsync();

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = "audience",
                ValidateIssuer = true,
                ValidIssuer = "issuer",
                IssuerSigningKeys = config.SigningKeys,
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