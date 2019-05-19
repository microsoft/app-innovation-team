using System;
using System.Collections.Generic;

namespace BotApp.Extensions.BotBuilder.QnAMaker.Accessors
{
    public class QnAMakerAccessor
    {
        public QnAMakerAccessor(Dictionary<string, Microsoft.Bot.Builder.AI.QnA.QnAMaker> qnaServices)
        {
            QnAMakerServices = qnaServices ?? throw new ArgumentNullException(nameof(qnaServices));
        }

        public Dictionary<string, Microsoft.Bot.Builder.AI.QnA.QnAMaker> QnAMakerServices { get; }
    }
}