using BotApp.Extensions.BotBuilder.ActiveDirectory.Domain;
using Microsoft.Bot.Builder;
using System.Threading.Tasks;

namespace BotApp.Extensions.BotBuilder.ActiveDirectory.Services
{
    public interface IActiveDirectoryService
    {
        ActiveDirectoryConfig GetConfiguration();

        Task<bool> ValidateTokenAsync(ITurnContext turnContext);
    }
}