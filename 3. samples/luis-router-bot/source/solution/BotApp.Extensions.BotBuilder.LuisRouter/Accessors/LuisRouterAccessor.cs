using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using System;
using System.Collections.Generic;

namespace BotApp.Extensions.BotBuilder.LuisRouter.Accessors
{
    public class LuisRouterAccessor
    {
        public LuisRouterAccessor(UserState userState, Dictionary<string, LuisRecognizer> luisServices)
        {
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
            LuisServices = luisServices ?? throw new ArgumentNullException(nameof(luisServices));
        }

        public UserState UserState { get; }
        public IStatePropertyAccessor<string> TokenPreference { get; set; }
        public Dictionary<string, LuisRecognizer> LuisServices { get; }
    }
}