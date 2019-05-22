using BotApp.Extensions.BotBuilder.LuisRouter.Domain;
using BotApp.Extensions.BotBuilder.LuisRouter.Services;
using BotApp.Extensions.BotBuilder.QnAMaker.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public class LuisQnADialog : ComponentDialog
    {
        private BotAccessor accessor = null;
        private ILuisRouterService luisRouterService = null;
        private IQnAMakerService qnaMakerService = null;

        public LuisQnADialog(BotAccessor accessor, ILuisRouterService luisRouterService, IQnAMakerService qnaMakerService) : base(nameof(LuisQnADialog))
        {
            this.accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
            this.luisRouterService = luisRouterService ?? throw new ArgumentNullException(nameof(luisRouterService));
            this.qnaMakerService = qnaMakerService ?? throw new ArgumentNullException(nameof(qnaMakerService));

            AddDialog(new WaterfallDialog(nameof(LuisQnADialog), new WaterfallStep[]
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
                await promptContext.Context.SendCustomResponseAsync(message);
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
                await promptContext.Context.SendCustomResponseAsync(message);
            }
            else
            {
                var value = promptContext.Recognized.Value;
                if (value.Index == 0)
                {
                    await accessor.AskForExamplePreference.SetAsync(promptContext.Context, true);
                    await accessor.ConversationState.SaveChangesAsync(promptContext.Context, false, cancellationToken);
                    return true;
                }
                else if (value.Index == 1)
                {
                    await accessor.AskForExamplePreference.SetAsync(promptContext.Context, false);
                    await accessor.ConversationState.SaveChangesAsync(promptContext.Context, false, cancellationToken);
                    return true;
                }
                else
                {
                    var message = $"Sorry, please answer correctly";
                    await promptContext.Context.SendCustomResponseAsync(message);
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

            if (question == "/start")
            {
                await step.EndDialogAsync();
                return Dialog.EndOfTurn;
            }

            var apps = await luisRouterService.LuisDiscoveryAsync(step, step.Context.Activity.Text, Startup.ApplicationCode, Startup.EncryptionKey);

            if (apps.Count > 0)
            {
                LuisAppDetail app = apps.OrderByDescending(x => x.Score).FirstOrDefault();

                var recognizerResult = await luisRouterService.LuisServices[app.Name].RecognizeAsync(step.Context, cancellationToken);
                var topIntent = recognizerResult?.GetTopScoringIntent();
                if (topIntent != null && topIntent.HasValue && topIntent.Value.score >= .90 && topIntent.Value.intent != "None")
                {
                    step.Context.Activity.Text = topIntent.Value.intent;

                    var qnaName = string.Empty;
                    qnaName = qnaMakerService.GetConfiguration().Name;

                    var response = await qnaMakerService.QnAMakerServices[qnaName].GetAnswersAsync(step.Context);
                    if (response != null && response.Length > 0)
                    {
                        string responseType = string.Empty;
                        responseType = FindResponseTypeMetadata(response[0].Metadata);
                        await step.Context.SendCustomResponseAsync(response[0].Answer, responseType);

                        if (!string.IsNullOrEmpty(responseType))
                        {
                            if (!topIntent.Value.intent.EndsWith("_Sample"))
                            {
                                List<Choice> choices = new List<Choice>();
                                choices.Add(new Choice { Value = $"Yes" });
                                choices.Add(new Choice { Value = $"No" });

                                var message = $"Would you like to see an example?";
                                await step.Context.SendCustomResponseAsync(message);

                                PromptOptions options = new PromptOptions { Choices = choices };
                                return await step.PromptAsync("AskForExampleValidator", options, cancellationToken: cancellationToken);
                            }
                        }
                    }
                    else
                    {
                        await accessor.AskForExamplePreference.SetAsync(step.Context, false);
                        await accessor.ConversationState.SaveChangesAsync(step.Context, false, cancellationToken);

                        var message = $"I did not find information to show you";
                        await step.Context.SendCustomResponseAsync(message);
                    }
                }
                else
                {
                    await accessor.AskForExamplePreference.SetAsync(step.Context, false);
                    await accessor.ConversationState.SaveChangesAsync(step.Context, false, cancellationToken);

                    var message = $"I did not find information to show you";
                    await step.Context.SendCustomResponseAsync(message);
                }
            }
            else
            {
                await accessor.AskForExamplePreference.SetAsync(step.Context, false);
                await accessor.ConversationState.SaveChangesAsync(step.Context, false, cancellationToken);

                var message = $"I did not find information to show you";
                await step.Context.SendCustomResponseAsync(message);
            }

            return await step.NextAsync();
        }

        private async Task<DialogTurnResult> ProcessIfExampleIsRequiredDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            bool askForExample = await accessor.AskForExamplePreference.GetAsync(step.Context, () => { return false; });

            if (askForExample)
            {
                var message = $"i would like to see a sample about {step.ActiveDialog.State["question"]}";
                //await step.Context.SendActivityAsync(message, cancellationToken: cancellationToken);
                step.Context.Activity.Text = message;

                var apps = await luisRouterService.LuisDiscoveryAsync(step, step.Context.Activity.Text, Startup.ApplicationCode, Startup.EncryptionKey);

                if (apps.Count > 0)
                {
                    LuisAppDetail app = apps.OrderByDescending(x => x.Score).FirstOrDefault();

                    var recognizerResult = await luisRouterService.LuisServices[app.Name].RecognizeAsync(step.Context, cancellationToken);
                    var topIntent = recognizerResult?.GetTopScoringIntent();
                    if (topIntent != null && topIntent.HasValue && topIntent.Value.score >= .90 && topIntent.Value.intent != "None")
                    {
                        step.Context.Activity.Text = topIntent.Value.intent;

                        var qnaName = string.Empty;
                        qnaName = qnaMakerService.GetConfiguration().Name;

                        var response = await qnaMakerService.QnAMakerServices[qnaName].GetAnswersAsync(step.Context);
                        if (response != null && response.Length > 0)
                        {
                            string responseType = string.Empty;
                            responseType = FindResponseTypeMetadata(response[0].Metadata);
                            await step.Context.SendCustomResponseAsync(response[0].Answer, responseType);
                        }
                        else
                        {
                            await accessor.AskForExamplePreference.SetAsync(step.Context, false);
                            await accessor.ConversationState.SaveChangesAsync(step.Context, false, cancellationToken);

                            message = $"I did not find information to show you";
                            await step.Context.SendCustomResponseAsync(message);
                        }
                    }
                    else
                    {
                        await accessor.AskForExamplePreference.SetAsync(step.Context, false);
                        await accessor.ConversationState.SaveChangesAsync(step.Context, false, cancellationToken);

                        message = $"I did not find information to show you";
                        await step.Context.SendCustomResponseAsync(message);
                    }
                }
                else
                {
                    await accessor.AskForExamplePreference.SetAsync(step.Context, false);
                    await accessor.ConversationState.SaveChangesAsync(step.Context, false, cancellationToken);

                    message = $"I did not find information to show you";
                    await step.Context.SendCustomResponseAsync(message);
                }
            }

            return await step.NextAsync();
        }

        private async Task<DialogTurnResult> EndDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            await accessor.AskForExamplePreference.SetAsync(step.Context, false);
            await accessor.ConversationState.SaveChangesAsync(step.Context, false, cancellationToken);

            await step.EndDialogAsync(step.ActiveDialog.State);
            await step.BeginDialogAsync(nameof(LuisQnADialog));

            return Dialog.EndOfTurn;
        }
    }
}