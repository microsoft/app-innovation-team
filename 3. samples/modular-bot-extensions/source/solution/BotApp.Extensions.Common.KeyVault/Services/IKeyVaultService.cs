using BotApp.Extensions.Common.KeyVault.Domain;
using System.Threading.Tasks;

namespace BotApp.Extensions.Common.KeyVault.Services
{
    public interface IKeyVaultService
    {
        KeyVaultConfig GetConfiguration();

        Task SetVaultKeyAsync(string secretKey, string secretValue);

        Task<string> GetVaultKeyAsync(string secretKey);
    }
}