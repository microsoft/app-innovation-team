using BotApp.Extensions.BotBuilder.Channel.WebChat.Domain;
using System.Threading.Tasks;

namespace BotApp.Extensions.BotBuilder.Channel.WebChat.Services
{
    public interface IWebChatService
    {
        WebChatConfig GetConfiguration();

        Task<GenerateResult> GetDirectLineTokenAsync(string secret);
    }
}