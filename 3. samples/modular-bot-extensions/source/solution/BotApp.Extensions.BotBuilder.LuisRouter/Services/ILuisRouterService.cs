using BotApp.Extensions.BotBuilder.LuisRouter.Domain;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BotApp.Extensions.BotBuilder.LuisRouter.Services
{
    public interface ILuisRouterService
    {
        UserState UserState { get; }

        IStatePropertyAccessor<string> TokenPreference { get; set; }

        Dictionary<string, LuisRecognizer> LuisServices { get; }

        LuisRouterConfig GetConfiguration();

        Task GetTokenAsync(WaterfallStepContext step, string encryptedRequest);

        Task<List<LuisAppDetail>> LuisDiscoveryAsync(WaterfallStepContext step, string text, string applicationCode, string encryptionKey);
    }
}