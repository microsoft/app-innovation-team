using BotApp.Extensions.BotBuilder.Channel.WebChat.Domain;

namespace BotApp.Extensions.BotBuilder.Channel.WebChat.Services
{
    public interface IWebChatService
    {
        WebChatConfig GetConfiguration();
    }
}