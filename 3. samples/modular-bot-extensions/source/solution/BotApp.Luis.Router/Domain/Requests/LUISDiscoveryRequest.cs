namespace BotApp.Luis.Router.Domain.Requests
{
    public class LuisDiscoveryRequest
    {
        public string Text { get; set; }
        public string BingSpellCheckSubscriptionKey { get; set; }
        public bool EnableLuisTelemetry { get; set; }
    }
}