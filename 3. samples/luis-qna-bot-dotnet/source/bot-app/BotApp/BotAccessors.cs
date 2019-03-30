using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;

namespace BotApp
{
    public class BotAccessors
    {
        public BotAccessors(ConversationState conversationState, UserState userState, Dictionary<string, LuisRecognizer> luisServices, Dictionary<string, QnAMaker> qnaServices)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
            LuisServices = luisServices ?? throw new ArgumentNullException(nameof(luisServices));
            QnAServices = qnaServices ?? throw new ArgumentNullException(nameof(qnaServices));
        }

        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }

        public IStatePropertyAccessor<bool> AskForExamplePreference { get; set; }

        public ConversationState ConversationState { get; }

        public UserState UserState { get; }

        public Dictionary<string, LuisRecognizer> LuisServices { get; }

        public Dictionary<string, QnAMaker> QnAServices { get; }
    }
}