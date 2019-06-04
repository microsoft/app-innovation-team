using BotApp.Extensions.BotBuilder.LuisRouter.Services;
using BotApp.Extensions.BotBuilder.QnAMaker.Services;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public class MainDialog : ComponentDialog
    {
        private BotAccessor accessor = null;
        private ILuisRouterService luisRouterService = null;
        private IQnAMakerService qnaMakerService = null;

        public MainDialog(BotAccessor accessor, ILuisRouterService luisRouterService, IQnAMakerService qnaMakerService) : base(nameof(MainDialog))
        {
            this.accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            this.luisRouterService = luisRouterService ?? throw new ArgumentNullException(nameof(luisRouterService));
            this.qnaMakerService = qnaMakerService ?? throw new ArgumentNullException(nameof(qnaMakerService));

            AddDialog(new WaterfallDialog(nameof(MainDialog), new WaterfallStep[]
            {
                LaunchLuisQnADialog,
                EndMainDialog
            }));

            AddDialog(new LuisQnADialog(accessor, luisRouterService, qnaMakerService));
        }

        private async Task<DialogTurnResult> LaunchLuisQnADialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            bool isAuthenticated = await accessor.IsAuthenticatedPreference.GetAsync(step.Context, () => { return false; });

            if (!isAuthenticated)
            {
                //is authenticated
                await accessor.IsAuthenticatedPreference.SetAsync(step.Context, true);

                return await step.BeginDialogAsync(nameof(LuisQnADialog));
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

            await step.BeginDialogAsync(nameof(LuisQnADialog), null, cancellationToken);

            return Dialog.EndOfTurn;
        }
    }
}