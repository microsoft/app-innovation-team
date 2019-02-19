using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;

namespace WBD
{
    public class BotAccessors
    {
        public BotAccessors(ConversationState conversationState, UserState userState, Dictionary<string, LuisRecognizer> luisServices)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
            LuisServices = luisServices ?? throw new ArgumentNullException(nameof(luisServices));
        }

        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }

        public IStatePropertyAccessor<string> LanguagePreference { get; set; }

        public IStatePropertyAccessor<bool> IsReadyForLUISPreference { get; set; }

        public ConversationState ConversationState { get; }

        public UserState UserState { get; }

        public Dictionary<string, LuisRecognizer> LuisServices { get; }
    }
}