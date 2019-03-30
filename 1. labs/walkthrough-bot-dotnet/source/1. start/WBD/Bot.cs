using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WBD
{
    public class Bot : IBot
    {
        private DialogSet dialogs = null;
        private BotAccessors accessors = null;

        public Bot(BotAccessors accessors)
        {
            this.accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            this.dialogs = new DialogSet(accessors.ConversationDialogState);
            this.dialogs.Add(new MainDialog(accessors));
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }

            var dialogContext = await dialogs.CreateContextAsync(turnContext, cancellationToken);

            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.ConversationUpdate:
                    if (turnContext.Activity.MembersAdded.FirstOrDefault()?.Id == turnContext.Activity.Recipient.Id)
                    {
                        // TODO: BEGIN INITIAL DIALOG
                    }
                    break;

                case ActivityTypes.Message:
                    var results = await dialogContext.ContinueDialogAsync(cancellationToken);

                    var text = turnContext.Activity.Text;
                    if (text == "/start")
                    {
                        // TODO: RESET DIALOG
                    }
                    else
                    {
                        if (!dialogContext.Context.Responded)
                        {
                            bool isReady = await accessors.IsReadyForLUISPreference.GetAsync(turnContext, () => { return false; });

                            if (!isReady)
                            {
                                await dialogContext.BeginDialogAsync("MainDialog", null, cancellationToken);
                            }
                            else
                            {
                                // TODO: CONFIGURE LUIS
                            }
                        }
                    }

                    break;
            }
        }

        private async Task ProcessIntentAsync(ITurnContext turnContext, string intent, double score, CancellationToken cancellationToken = default(CancellationToken))
        {
            var reply = turnContext.Activity.CreateReply();
            reply.Attachments = new List<Attachment>();

            AnimationCard animationCard = null;

            switch (intent)
            {
                case "Calendar_Add":

                    // TODO: ADD ANIMATION CARD

                    break;

                case "Calendar_Find":

                    animationCard = new AnimationCard
                    {
                        Title = $"Intent: {intent}",
                        Subtitle = $"Score: {score}",
                        Media = new List<MediaUrl>
                        {
                            new MediaUrl()
                            {
                                Url = "https://i.gifer.com/7Knv.gif",
                            },
                        },
                    };

                    reply.Attachments.Add(animationCard.ToAttachment());

                    break;
            }

            await turnContext.SendActivityAsync(reply, cancellationToken);
        }
    }
}