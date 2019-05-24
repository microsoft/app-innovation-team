using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Linq;

namespace BotApp.Identity.Tests
{
    public class JwtValidator
    {
        public bool Validate(string token)
        {
            var validationSucceeded = true;
            try
            {
                InternalValidation(token);
            }
            catch (SecurityTokenException ex)
            {
                Console.WriteLine(ex.Message);
                validationSucceeded = false;
            }

            return validationSucceeded;
        }


        private JwtSecurityToken InternalValidation(string token)
        {
            string stsDiscoveryEndpoint = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";

            ConfigurationManager<OpenIdConnectConfiguration> configManager = new ConfigurationManager<OpenIdConnectConfiguration>(stsDiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());
            OpenIdConnectConfiguration config = configManager.GetConfigurationAsync().Result;

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = "https://graph.microsoft.com",
                ValidateIssuer = true,
                ValidIssuer = "https://sts.windows.net/3bb57ff3-b0fc-4888-81a3-16f8f218a320/",
                //IssuerSigningKeys = config.SigningKeys,
                IssuerSigningKey = config.SigningKeys.ToList()[1],
                ValidateIssuerSigningKey = false,
                ValidateLifetime = true
            };

            JwtSecurityTokenHandler tokendHandler = new JwtSecurityTokenHandler();

            SecurityToken jwt;
            IdentityModelEventSource.ShowPII = true;
            var result = tokendHandler.ValidateToken(token, validationParameters, out jwt);

            return jwt as JwtSecurityToken;
        }

        private static string Base64UrlDecode(string value, Encoding encoding = null)
        {
            string urlDecodedValue = value.Replace('_', '/').Replace('-', '+');

            switch (value.Length % 4)
            {
                case 2:
                    urlDecodedValue += "==";
                    break;
                case 3:
                    urlDecodedValue += "=";
                    break;
            }

            return Encoding.ASCII.GetString(Convert.FromBase64String(urlDecodedValue));
        }
    }
}
