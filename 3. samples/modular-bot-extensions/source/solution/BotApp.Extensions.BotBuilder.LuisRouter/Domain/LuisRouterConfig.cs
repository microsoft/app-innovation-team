using System.Collections.Generic;

namespace BotApp.Extensions.BotBuilder.LuisRouter.Domain
{
    public class LuisApp
    {
        public string Name { get; set; }
        public string AppId { get; set; }
        public string AuthoringKey { get; set; }
        public string Endpoint { get; set; }
    }

    public class LuisRouterConfig
    {
        public string LuisRouterUrl { get; set; }
        public string BingSpellCheckSubscriptionKey { get; set; }
        public bool EnableLuisTelemetry { get; set; }
        public IEnumerable<LuisApp> LuisApplications { get; set; }
    }
}