using Microsoft.Bot.Builder.AI.Luis;
using System.Collections.Generic;

namespace BotApp.LUIS.Middleware.Domain.Settings
{
    public class Settings
    {
        public static string AuthorizationKey { get; set; }
        public static List<LuisAppRegistration> LuisAppRegistrations { get; set; }
        public static Dictionary<string, LuisRecognizer> LuisServices { get; set; }
    }
}