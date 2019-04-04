using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;

namespace BotApp
{
    public class MainDialog : ComponentDialog
    {
        private const string dialogId = "MainDialog";
        private BotAccessors accessors = null;
        private CustomResponseHelper customResponseHelper = null;

        public MainDialog(BotAccessors accessors) : base(dialogId)
        {
            this.accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));
            this.customResponseHelper = new CustomResponseHelper();

            AddDialog(new WaterfallDialog(dialogId, new WaterfallStep[]
            {
                AskQuestionDialog,
                ProcessQuestionDialog,
                ProcessIfExampleIsRequiredDialog,
                EndDialog
            }));

            AddDialog(new TextPrompt("QuestionValidator", QuestionValidator));
            AddDialog(new ChoicePrompt("AskForExampleValidator", AskForExampleValidator) { Style = ListStyle.List });
        }

        private string FindResponseTypeMetadata(Metadata[] metadata)
        {
            string result = string.Empty;

            if (metadata.Length > 0)
            {
                foreach (Metadata m in metadata)
                {
                    if (m.Name == "responsetype")
                    {
                        result = m.Value;
                        break;
                    }
                }
            }

            return result;
        }

        private async Task<bool> QuestionValidator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (promptContext.Recognized.Value == null)
            {
                var message = $"Sorry, please answer correctly";
                await customResponseHelper.SendActivityAsync(promptContext.Context, cancellationToken, "text", message);
            }
            else
            {
                return true;
            }

            return false;
        }

        private async Task<bool> AskForExampleValidator(PromptValidatorContext<FoundChoice> promptContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (promptContext.Recognized.Value == null)
            {
                var message = $"Sorry, please answer correctly";
                await customResponseHelper.SendActivityAsync(promptContext.Context, cancellationToken, "text", message);
            }
            else
            {
                var value = promptContext.Recognized.Value;
                if (value.Index == 0)
                {
                    await accessors.AskForExamplePreference.SetAsync(promptContext.Context, true);
                    await accessors.ConversationState.SaveChangesAsync(promptContext.Context, false, cancellationToken);
                    return true;
                }
                else if (value.Index == 1)
                {
                    await accessors.AskForExamplePreference.SetAsync(promptContext.Context, false);
                    await accessors.ConversationState.SaveChangesAsync(promptContext.Context, false, cancellationToken);
                    return true;
                }
                else
                {
                    var message = $"Sorry, please answer correctly";
                    await customResponseHelper.SendActivityAsync(promptContext.Context, cancellationToken, "text", message);
                }
            }

            return false;
        }

        private async Task<DialogTurnResult> AskQuestionDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            var options = new PromptOptions
            {
                Prompt = new Activity { Type = ActivityTypes.Message, Text = $"What topic would you like to know more about?" }
            };
            return await step.PromptAsync("QuestionValidator", options, cancellationToken);
        }

        private async Task<DialogTurnResult> ProcessQuestionDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            var question = (string)step.Result;
            step.ActiveDialog.State["question"] = question;

            var recognizerResult = await accessors.LuisServices[Settings.LuisName01].RecognizeAsync(step.Context, cancellationToken);
            var topIntent = recognizerResult?.GetTopScoringIntent();
            if (topIntent != null && topIntent.HasValue && topIntent.Value.score >= .80 && topIntent.Value.intent != "None")
            {
                step.Context.Activity.Text = topIntent.Value.intent;

                var response = await accessors.QnAServices[Settings.QnAName01].GetAnswersAsync(step.Context);
                if (response != null && response.Length > 0)
                {
                    string responseType = string.Empty;
                    responseType = FindResponseTypeMetadata(response[0].Metadata);
                    await customResponseHelper.SendActivityAsync(step.Context, cancellationToken, responseType, response[0].Answer);

                    if (!string.IsNullOrEmpty(responseType))
                    {
                        if (!topIntent.Value.intent.EndsWith("_Sample"))
                        {
                            List<Choice> choices = new List<Choice>();
                            choices.Add(new Choice { Value = $"Yes" });
                            choices.Add(new Choice { Value = $"No" });

                            var message = $"Would you like to see an example?";
                            await customResponseHelper.SendActivityAsync(step.Context, cancellationToken, "text", message);

                            PromptOptions options = new PromptOptions { Choices = choices };
                            return await step.PromptAsync("AskForExampleValidator", options, cancellationToken: cancellationToken);
                        }
                    }
                }
                else
                {
                    await accessors.AskForExamplePreference.SetAsync(step.Context, false);
                    await accessors.ConversationState.SaveChangesAsync(step.Context, false, cancellationToken);

                    var message = $"I did not find information to show you";
                    await customResponseHelper.SendActivityAsync(step.Context, cancellationToken, "text", message);
                }
            }
            else
            {
                await accessors.AskForExamplePreference.SetAsync(step.Context, false);
                await accessors.ConversationState.SaveChangesAsync(step.Context, false, cancellationToken);

                var message = $"I did not find information to show you";
                await customResponseHelper.SendActivityAsync(step.Context, cancellationToken, "text", message);
            }

            return await step.NextAsync();
        }

        private async Task<DialogTurnResult> ProcessIfExampleIsRequiredDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            bool askForExample = await accessors.AskForExamplePreference.GetAsync(step.Context, () => { return false; });

            if (askForExample)
            {
                var message = $"i would like to see a sample about {step.ActiveDialog.State["question"]}";
                //await step.Context.SendActivityAsync(message, cancellationToken: cancellationToken);
                step.Context.Activity.Text = message;

                var recognizerResult = await accessors.LuisServices[Settings.LuisName01].RecognizeAsync(step.Context, cancellationToken);
                var topIntent = recognizerResult?.GetTopScoringIntent();
                if (topIntent != null && topIntent.HasValue && topIntent.Value.score >= .80 && topIntent.Value.intent != "None")
                {
                    step.Context.Activity.Text = topIntent.Value.intent;

                    var response = await accessors.QnAServices[Settings.QnAName01].GetAnswersAsync(step.Context);
                    if (response != null && response.Length > 0)
                    {
                        string responseType = string.Empty;
                        responseType = FindResponseTypeMetadata(response[0].Metadata);
                        await customResponseHelper.SendActivityAsync(step.Context, cancellationToken, responseType, response[0].Answer);
                    }
                    else
                    {
                        await accessors.AskForExamplePreference.SetAsync(step.Context, false);
                        await accessors.ConversationState.SaveChangesAsync(step.Context, false, cancellationToken);

                        message = $"I did not find information to show you";
                        await customResponseHelper.SendActivityAsync(step.Context, cancellationToken, "text", message);
                    }
                }
                else
                {
                    await accessors.AskForExamplePreference.SetAsync(step.Context, false);
                    await accessors.ConversationState.SaveChangesAsync(step.Context, false, cancellationToken);

                    message = $"I did not find information to show you";
                    await customResponseHelper.SendActivityAsync(step.Context, cancellationToken, "text", message);
                }
            }

            return await step.NextAsync();
        }

        private async Task<DialogTurnResult> EndDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            await accessors.AskForExamplePreference.SetAsync(step.Context, false);
            await accessors.ConversationState.SaveChangesAsync(step.Context, false, cancellationToken);

            await step.EndDialogAsync(step.ActiveDialog.State);
            await step.BeginDialogAsync(dialogId);

            customResponseHelper.Dispose();

            return Dialog.EndOfTurn;
        }
    }
}