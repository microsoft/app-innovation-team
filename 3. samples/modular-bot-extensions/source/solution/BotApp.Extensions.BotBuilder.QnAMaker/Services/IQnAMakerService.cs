using BotApp.Extensions.BotBuilder.QnAMaker.Domain;
using System.Collections.Generic;

namespace BotApp.Extensions.BotBuilder.QnAMaker.Services
{
    public interface IQnAMakerService
    {
        Dictionary<string, Microsoft.Bot.Builder.AI.QnA.QnAMaker> QnAMakerServices { get; }

        QnAMakerConfig GetConfiguration();
    }
}