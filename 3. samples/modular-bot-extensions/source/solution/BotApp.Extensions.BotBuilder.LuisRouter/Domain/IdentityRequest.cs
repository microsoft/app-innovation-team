using System;

namespace BotApp.Extensions.BotBuilder.LuisRouter.Domain
{
    public class IdentityRequest
    {
        public string appcode { get; set; }
        public DateTime timestamp { get; set; }
    }
}