using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Logging;
using System;

namespace BotApp
{
    public class BotAccessors
    {
        public BotAccessors(ILoggerFactory loggerFactory,
            ConversationState conversationState,
            UserState userState)
        {
            LoggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }

        public ILoggerFactory LoggerFactory { get; set; }

        public IStatePropertyAccessor<DialogState> ConversationDialogState { get; set; }

        public IStatePropertyAccessor<bool> AskForExamplePreference { get; set; }

        public IStatePropertyAccessor<bool> IsAuthenticatedPreference { get; set; }

        public ConversationState ConversationState { get; }

        public UserState UserState { get; }
    }
}