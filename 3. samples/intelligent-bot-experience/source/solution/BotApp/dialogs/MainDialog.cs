using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public class MainDialog : ComponentDialog
    {
        public const string dialogId = "MainDialog";
        private BotAccessors accessors = null;

        public MainDialog(BotAccessors accessors) : base(dialogId)
        {
            this.accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            AddDialog(new WaterfallDialog(dialogId, new WaterfallStep[]
            {
                LaunchIdentifyDialog,
                EndMainDialog
            }));

            AddDialog(new IdentifyDialog(accessors));
            AddDialog(new LuisQnADialog(accessors));
        }

        private async Task<DialogTurnResult> LaunchIdentifyDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            bool isAuthenticated = await accessors.IsAuthenticatedPreference.GetAsync(step.Context, () => { return false; });

            if (!isAuthenticated)
            {
                return await step.BeginDialogAsync(IdentifyDialog.dialogId);
            }
            else
            {
                return await step.NextAsync();
            }
        }

        private async Task<DialogTurnResult> EndMainDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            string userName = await accessors.NamePreference.GetAsync(step.Context, () => { return string.Empty; });

            var message = $"Hey {userName}, thanks for using this bot, now you will be redirect to use LUIS and QnA dialog to perform intelligent interactions";
            await step.Context.SendActivityAsync($"{message}");

            //ending dialog
            await step.EndDialogAsync(step.ActiveDialog.State);

            await step.BeginDialogAsync(LuisQnADialog.dialogId, null, cancellationToken);

            return Dialog.EndOfTurn;
        }
    }
}