using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace BotApp
{
    public class BotAccessors
    {
        public BotAccessors(ILoggerFactory loggerFactory,
            ConversationState conversationState,
            UserState userState,
            Dictionary<string, LuisRecognizer> luisServices,
            Dictionary<string, QnAMaker> qnaServices)
        {
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
            LuisServices = luisServices ?? throw new ArgumentNullException(nameof(luisServices));
            QnAServices = qnaServices ?? throw new ArgumentNullException(nameof(qnaServices));
        }

        public ILoggerFactory LoggerFactory { get; set; }

        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }

        public IStatePropertyAccessor<bool> AskForExamplePreference { get; set; }

        public IStatePropertyAccessor<string> DetectedFaceIdPreference { get; set; }

        public IStatePropertyAccessor<string> FullnamePreference { get; set; }

        public IStatePropertyAccessor<string> NamePreference { get; set; }

        public IStatePropertyAccessor<string> LastnamePreference { get; set; }

        public IStatePropertyAccessor<bool> IsNewPreference { get; set; }

        public IStatePropertyAccessor<string> ImageUriPreference { get; set; }

        public IStatePropertyAccessor<string> HashPreference { get; set; }

        public IStatePropertyAccessor<bool> IsAuthenticatedPreference { get; set; }

        public ConversationState ConversationState { get; }

        public UserState UserState { get; }

        public Dictionary<string, LuisRecognizer> LuisServices { get; }

        public Dictionary<string, QnAMaker> QnAServices { get; }
    }
}