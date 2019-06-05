using System.Collections.Generic;

namespace BotApp.Extensions.BotBuilder.LuisRouter.Domain
{
    public class LuisRouterConfig
    {
        public string LuisRouterUrl { get; set; }
        public string BingSpellCheckSubscriptionKey { get; set; }
        public IEnumerable<LuisApp> LuisApplications { get; set; }
    }
}