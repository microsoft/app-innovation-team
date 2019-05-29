// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using BotApp.Extensions.BotBuilder.ActiveDirectory.Services;
using BotApp.Extensions.BotBuilder.LuisRouter.Services;
using BotApp.Extensions.BotBuilder.QnAMaker.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public class BotAppBot : ActivityHandler
    {
        protected readonly Dialog dialog;
        protected readonly BotState conversationState;
        protected readonly BotState userState;
        protected readonly ILogger logger;
        protected BotAccessor accessor = null;
        protected DialogSet dialogs = null;
        protected ILuisRouterService luisRouterService = null;
        protected IQnAMakerService qnaMakerService = null;
        protected IActiveDirectoryService activeDirectoryService = null;

        public BotAppBot(BotAccessor accessor, ILuisRouterService luisRouterService, IQnAMakerService qnaMakerService, IActiveDirectoryService activeDirectoryService, ConversationState conversationState, UserState userState, ILogger<BotAppBot> logger)
        {
            this.accessor = accessor;
            this.conversationState = conversationState;
            this.userState = userState;
            this.logger = logger;
            this.luisRouterService = luisRouterService;
            this.qnaMakerService = qnaMakerService;
            this.activeDirectoryService = activeDirectoryService;

            this.dialogs = new DialogSet(accessor.ConversationDialogState);
            this.dialogs.Add(new MainDialog(accessor, luisRouterService, qnaMakerService));
            this.dialogs.Add(new LuisQnADialog(accessor, luisRouterService, qnaMakerService));
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
                        // begin: token validation
                        bool hasPermission = await activeDirectoryService.ValidateTokenAsync(turnContext);

                        if (!hasPermission)
                        {
                            var message = string.Empty;
                            message = $"Ooops!! it seems you don't have permission to talk with me";
                            await turnContext.SendCustomResponseAsync(message);
                            return;
                        }

                        // end: token validation

                        await LaunchWelcomeAsync(turnContext);
                        await dialogContext.BeginDialogAsync(nameof(MainDialog), null, cancellationToken: cancellationToken);
                    }
                    break;

                case ActivityTypes.Message:
                    var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                    var text = turnContext.Activity.Text;
                    if (text == "/start")
                    {
                        await this.accessor.AskForExamplePreference.DeleteAsync(turnContext);
                        await this.accessor.ConversationDialogState.DeleteAsync(turnContext);
                        await this.accessor.IsAuthenticatedPreference.DeleteAsync(turnContext);
                        await this.luisRouterService.TokenPreference.DeleteAsync(turnContext);
                        await dialogContext.EndDialogAsync();
                        await dialogContext.BeginDialogAsync(nameof(MainDialog), null, cancellationToken);
                    }
                    else
                    {
                        if (!dialogContext.Context.Responded)
                        {
                            bool isAuthenticated = await this.accessor.IsAuthenticatedPreference.GetAsync(turnContext, () => { return false; });

                            if (!isAuthenticated)
                            {
                                await dialogContext.BeginDialogAsync(nameof(MainDialog), null, cancellationToken);
                            }
                            else
                            {
                                await dialogContext.BeginDialogAsync(nameof(LuisQnADialog), null, cancellationToken);
                            }
                        }
                    }
                    break;
            }

            // Save any state changes that might have occured during the turn.
            await this.conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
            await this.userState.SaveChangesAsync(turnContext, false, cancellationToken);
        }
    }
}