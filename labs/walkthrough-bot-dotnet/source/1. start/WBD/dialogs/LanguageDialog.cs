using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WBD
{
    public class LanguageDialog : ComponentDialog
    {
        private const string dialogId = "LanguageDialog";
        private BotAccessors accessors = null;

        public LanguageDialog(BotAccessors accessors) : base(dialogId)
        {
            this.accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            AddDialog(new WaterfallDialog("LanguageDialog", new WaterfallStep[]
            {
                // TODO: ADD WATERFALLDIALOGS
            }));

            AddDialog(new TextPrompt("TextPromptValidator", validator));
        }

        private async Task<bool> validator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO: ADD PROMPT VALIDATIONS

            return false;
        }

        private async Task<DialogTurnResult> RequestPhraseDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            var options = new PromptOptions
            {
                Prompt = new Activity { Type = ActivityTypes.Message, Text = $"Welcome to the walkthrough-bot-dotnet (version: {Settings.BotVersion}), write a phrase to identify your current language" },
                RetryPrompt = new Activity { Type = ActivityTypes.Message, Text = "Make sure the text is greater than four characters." }
            };
            return await step.PromptAsync("TextPromptValidator", options, cancellationToken);
        }

        private async Task<DialogTurnResult> ResponsePhraseDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            var phrase = (string)step.Result;
            var code = await TranslatorHelper.GetDesiredLanguageAsync(phrase);
            step.ActiveDialog.State["language"] = code;

            var response = "Remember, you can use the command {{00}} in case you want to start again the bot";
            var message = await TranslatorHelper.TranslateSentenceAsync(response, "en", step.ActiveDialog.State["language"].ToString());
            message = message.Replace("{{00}}", "/start");
            await step.Context.SendActivityAsync($"{message}");

            return await step.NextAsync();
        }

        private async Task<DialogTurnResult> EndLanguageDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            // TODO: SAVE DIALOG PREFERENCES

            //ending dialog
            return await step.EndDialogAsync(step.ActiveDialog.State);
        }
    }
}