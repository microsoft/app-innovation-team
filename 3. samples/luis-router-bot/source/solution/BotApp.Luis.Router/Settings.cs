using Microsoft.Bot.Builder.AI.Luis;
using System.Collections.Generic;

namespace BotApp.Luis.Router.Domain.Settings
{
    public class Settings
    {
        public static string AuthorizationKey { get; set; }
        public static List<LuisAppRegistration> LuisAppRegistrations { get; set; }
        public static Dictionary<string, LuisRecognizer> LuisServices { get; set; }
    }
}