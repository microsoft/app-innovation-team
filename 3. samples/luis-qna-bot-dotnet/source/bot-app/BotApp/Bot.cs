using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
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

        private async Task LaunchWelcomeAsync(ITurnContext turnContext)
        {
            var message = string.Empty;
            DateTime currentDateTime = DateTimeHelper.GetCustomTimeZone();

            if (currentDateTime.Hour >= 6)
                message = "Hello, good morning!";

            if (currentDateTime.Hour >= 12)
                message = "Hello, good afternoon!";

            if ((currentDateTime.Hour >= 0 && currentDateTime.Hour < 6) || currentDateTime.Hour >= 18)
                message = "Hello, good evening!";

            await turnContext.SendCustomResponseAsync(message);

            message = $"In Microsoft we believe in what people make possible, our mission is to empower every person and every organization on the planet to achieve more";
            await turnContext.SendCustomResponseAsync(message);

            message = $"I am a trained digital assistant from Microsoft trained to demonstrate how to work with LUIS and QnA simultaneously";
            await turnContext.SendCustomResponseAsync(message);
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
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
                        await dialogContext.BeginDialogAsync("MainDialog", null, cancellationToken: cancellationToken);
                    }
                    break;

                case ActivityTypes.Message:
                    var results = await dialogContext.ContinueDialogAsync(cancellationToken: cancellationToken);

                    if (!dialogContext.Context.Responded)
                    {
                        await dialogContext.BeginDialogAsync("MainDialog", null, cancellationToken);
                    }
                    break;
            }
        }
    }
}