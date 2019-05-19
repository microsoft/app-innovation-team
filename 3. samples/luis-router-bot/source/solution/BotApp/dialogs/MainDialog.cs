using BotApp.Extensions.BotBuilder.LuisRouter.Accessors;
using BotApp.Extensions.BotBuilder.QnAMaker.Accessors;
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
        private LuisRouterAccessor luisRouterAccessor = null;
        private QnAMakerAccessor qnaMakerAccessor = null;

        public MainDialog(BotAccessors accessors, LuisRouterAccessor luisRouterAccessor, QnAMakerAccessor qnaMakerAccessor) : base(dialogId)
        {
            this.accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            this.luisRouterAccessor = luisRouterAccessor ?? throw new ArgumentNullException(nameof(luisRouterAccessor));
            this.qnaMakerAccessor = qnaMakerAccessor ?? throw new ArgumentNullException(nameof(qnaMakerAccessor));

            AddDialog(new WaterfallDialog(dialogId, new WaterfallStep[]
            {
                LaunchLuisQnADialog,
                EndMainDialog
            }));

            AddDialog(new LuisQnADialog(accessors, luisRouterAccessor, qnaMakerAccessor));
        }

        private async Task<DialogTurnResult> LaunchLuisQnADialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            bool isAuthenticated = await accessors.IsAuthenticatedPreference.GetAsync(step.Context, () => { return false; });

            if (!isAuthenticated)
            {
                //is authenticated
                await accessors.IsAuthenticatedPreference.SetAsync(step.Context, true);

                return await step.BeginDialogAsync(LuisQnADialog.dialogId);
            }
            else
            {
                return await step.NextAsync();
            }
        }

        private async Task<DialogTurnResult> EndMainDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            var message = $"Hey, thanks for using this bot, now you will be redirect to use LUIS and QnA dialog to perform intelligent interactions";
            await step.Context.SendActivityAsync($"{message}");

            //ending dialog
            await step.EndDialogAsync(step.ActiveDialog.State);

            await step.BeginDialogAsync(LuisQnADialog.dialogId, null, cancellationToken);

            return Dialog.EndOfTurn;
        }
    }
}