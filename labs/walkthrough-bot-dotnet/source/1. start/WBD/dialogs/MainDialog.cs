using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WBD
{
    public class MainDialog : ComponentDialog
    {
        private const string dialogId = "MainDialog";
        private BotAccessors accessors = null;

        public MainDialog(BotAccessors accessors) : base(dialogId)
        {
            this.accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            AddDialog(new WaterfallDialog("MainDialog", new WaterfallStep[]
            {
                LaunchLanguageDialog,
                EndMainDialog
            }));

            AddDialog(new LanguageDialog(accessors));
        }

        private async Task<DialogTurnResult> LaunchLanguageDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            bool isReady = await accessors.IsReadyForLUISPreference.GetAsync(step.Context, () => { return false; });

            if (!isReady)
            {
                return await step.BeginDialogAsync("LanguageDialog");
            }
            else
            {
                return await step.NextAsync();
            }
        }

        private async Task<DialogTurnResult> EndMainDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            string userLanguage = await accessors.LanguagePreference.GetAsync(step.Context, () => { return string.Empty; });

            if (string.IsNullOrEmpty(userLanguage))
                throw new System.Exception("there was an issue reading the language preference");

            var response = $"Hey, thanks for using this bot, now you will be redirect to use LUIS to perform intelligent interactions";
            var message = await TranslatorHelper.TranslateSentenceAsync(response, "en", userLanguage);
            await step.Context.SendActivityAsync($"{message}");

            //ending dialog
            await step.EndDialogAsync(step.ActiveDialog.State);
            return Dialog.EndOfTurn;
        }
    }
}