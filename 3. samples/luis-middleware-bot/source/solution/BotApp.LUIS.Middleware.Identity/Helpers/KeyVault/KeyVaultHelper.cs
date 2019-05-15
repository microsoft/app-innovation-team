using BotApp.LUIS.Middleware.Identity.Domain.KeyVault;
using BotApp.LUIS.Middleware.Identity.Helpers.Base;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace BotApp.LUIS.Middleware.Identity.Helpers.KeyVault
{
    public class KeyVaultHelper : BaseHelper
    {
        private KeyVaultConnectionInfo KEY_VAULT_CONNECTION = null;

        public KeyVaultHelper(KeyVaultConnectionInfo keyVaultConnection)
        {
            this.KEY_VAULT_CONNECTION = keyVaultConnection;
        }

        public async Task SetVaultKeyAsync(string secretKey, string secretValue)
        {
            var keyclient = GetClient();
            var result = await keyclient.SetSecretAsync(KEY_VAULT_CONNECTION.KeyVaultIdentifier, secretKey, secretValue);
            string str_result = JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
            //Console.WriteLine($"Key: {secretKey} set completed: {str_result}\n");
        }

        public async Task<string> GetVaultKeyAsync(string secretKey)
        {
            var keyclient = GetClient();
            string secretUrl = $"{KEY_VAULT_CONNECTION.KeyVaultIdentifier}/secrets/{secretKey}";
            var secret = await keyclient.GetSecretAsync(secretUrl);
            return secret.Value;
        }

        private KeyVaultClient GetClient() => new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(async (string authority, string resource, string scope) =>
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            ClientCredential clientCred = new ClientCredential(KEY_VAULT_CONNECTION.ClientId, KEY_VAULT_CONNECTION.ClientSecret);
            var authResult = await context.AcquireTokenAsync(resource, clientCred);
            return authResult.AccessToken;
        }));
    }
}