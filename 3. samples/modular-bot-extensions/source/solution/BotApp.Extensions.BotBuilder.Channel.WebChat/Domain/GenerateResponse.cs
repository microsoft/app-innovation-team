namespace BotApp.Extensions.BotBuilder.Channel.WebChat.Domain
{
    public class GenerateResponse
    {
        public string conversationId { get; set; }
        public string token { get; set; }
        public int expires_in { get; set; }
    }
}