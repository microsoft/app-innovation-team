// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public class BotAppBot : ActivityHandler
    {
        protected readonly Dialog _dialog;
        protected readonly BotState _conversationState;
        protected readonly BotState _userState;
        protected readonly ILogger _logger;
        protected BotAccessors _accessors = null;
        protected DialogSet dialogs = null;

        public BotAppBot(BotAccessors accessors, ConversationState conversationState, UserState userState, ILogger<BotAppBot> logger)
        {
            _accessors = accessors;
            _conversationState = conversationState;
            _userState = userState;
            _logger = logger;

            this.dialogs = new DialogSet(accessors.ConversationDialogState);
            this.dialogs.Add(new MainDialog(accessors));
            this.dialogs.Add(new LuisQnADialog(accessors));
        }

        private async Task LaunchWelcomeAsync(ITurnContext turnContext)
        {
            var message = string.Empty;

            message = $"Hello stranger!, in Microsoft we believe in what people make possible, our mission is to empower every person and every organization on the planet to achieve more";
            await turnContext.SendCustomResponseAsync(message);

            message = $"I am a digital assistant from Microsoft, trained to demonstrate how to work with intelligent experiences with Bot Builder V4";
            await turnContext.SendCustomResponseAsync(message);
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var dialogContext = await dialogs.CreateContextAsync(turnContext, cancellationToken: cancellationToken);

            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    if (turnContext.Activity.MembersAdded.FirstOrDefault()?.Id == turnContext.Activity.Recipient.Id)
                    {
                        await LaunchWelcomeAsync(turnContext);
                        await dialogContext.BeginDialogAsync(MainDialog.dialogId, null, cancellationToken: cancellationToken);
                    }
                    break;

                case ActivityTypes.Message:
                    var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                    var text = turnContext.Activity.Text;
                    if (text == "/start")
                    {
                        await _accessors.AskForExamplePreference.DeleteAsync(turnContext);
                        await _accessors.ConversationDialogState.DeleteAsync(turnContext);
                        await _accessors.IsAuthenticatedPreference.DeleteAsync(turnContext);
                        await dialogContext.EndDialogAsync();
                        await dialogContext.BeginDialogAsync(MainDialog.dialogId, null, cancellationToken);
                    }
                    else
                    {
                        if (!dialogContext.Context.Responded)
                        {
                            bool isAuthenticated = await _accessors.IsAuthenticatedPreference.GetAsync(turnContext, () => { return false; });

                            if (!isAuthenticated)
                            {
                                await dialogContext.BeginDialogAsync(MainDialog.dialogId, null, cancellationToken);
                            }
                            else
                            {
                                await dialogContext.BeginDialogAsync(LuisQnADialog.dialogId, null, cancellationToken);
                            }
                        }
                    }
                    break;
            }

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}