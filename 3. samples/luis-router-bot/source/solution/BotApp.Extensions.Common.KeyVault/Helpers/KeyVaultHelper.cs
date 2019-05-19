using BotApp.Extensions.Common.KeyVault.Domain;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace BotApp.Extensions.Common.KeyVault.Helpers
{
    public class KeyVaultHelper : BaseHelper
    {
        private readonly KeyVaultConfig config = null;

        public KeyVaultHelper(string environmentName, string contentRootPath)
        {
            var builder = new ConfigurationBuilder()
              .SetBasePath(contentRootPath)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
              .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
              .AddEnvironmentVariables();

            var configuration = builder.Build();

            config = new KeyVaultConfig();
            configuration.GetSection("KeyVaultConfig").Bind(config);

            if (string.IsNullOrEmpty(config.CertificateName))
                throw new Exception("Missing value in KeyVaultConfig -> CertificateName");

            if (string.IsNullOrEmpty(config.ClientId))
                throw new Exception("Missing value in KeyVaultConfig -> ClientId");

            if (string.IsNullOrEmpty(config.ClientSecret))
                throw new Exception("Missing value in KeyVaultConfig -> ClientSecret");

            if (string.IsNullOrEmpty(config.Identifier))
                throw new Exception("Missing value in KeyVaultConfig -> Identifier");
        }

        public KeyVaultConfig GetConfiguration() => config;

        public async Task SetVaultKeyAsync(string secretKey, string secretValue)
        {
            var keyclient = GetClient();
            var result = await keyclient.SetSecretAsync(config.Identifier, secretKey, secretValue);
            string str_result = JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
        }

        public async Task<string> GetVaultKeyAsync(string secretKey)
        {
            var keyclient = GetClient();
            string secretUrl = $"{config.Identifier}/secrets/{secretKey}";
            var secret = await keyclient.GetSecretAsync(secretUrl);
            return secret.Value;
        }

        private KeyVaultClient GetClient() => new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(async (string authority, string resource, string scope) =>
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            ClientCredential clientCred = new ClientCredential(config.ClientId, config.ClientSecret);
            var authResult = await context.AcquireTokenAsync(resource, clientCred);
            return authResult.AccessToken;
        }));
    }
}