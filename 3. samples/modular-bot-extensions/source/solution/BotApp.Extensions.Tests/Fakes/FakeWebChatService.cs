using BotApp.Extensions.BotBuilder.Channel.WebChat.Domain;
using BotApp.Extensions.BotBuilder.Channel.WebChat.Services;

namespace BotApp.Extensions.Tests.Fakes
{
    public class FakeWebChatService : IWebChatService
    {
        public WebChatConfig GetConfiguration()
        {
            var webChatConfig = new WebChatConfig()
            {
                Secret = "secret"
            };
            return webChatConfig;
        }
    }
}