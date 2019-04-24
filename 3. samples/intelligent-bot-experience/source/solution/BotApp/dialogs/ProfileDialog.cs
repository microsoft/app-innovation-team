using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public class ProfileDialog : ComponentDialog
    {
        public const string dialogId = "ProfileDialog";
        private BotAccessors accessors = null;

        public ProfileDialog(BotAccessors accessors) : base(dialogId)
        {
            this.accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            AddDialog(new WaterfallDialog(dialogId, new WaterfallStep[]
            {
                AskFullnameDialog,
                GetFullnameDialog,
                ListCombinationsDialog,
                DecomposeNameLastnameDialog,
                EndProfileDialog
            }));

            AddDialog(new TextPrompt("TextPromptValidator", validator));
            AddDialog(new ChoicePrompt("ChoicePrompt") { Style = ListStyle.List });
        }

        private async Task<bool> validator(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (promptContext.Recognized.Value == null)
            {
                var message = $"Sorry, but I'm expecting an string, send me another phrase";
                await promptContext.Context.SendActivityAsync($"{message}");
            }
            else
            {
                var value = promptContext.Recognized.Value;
                if (value.Length < 10)
                {
                    var message = $"Your full name must be at least 10 characters long";
                    await promptContext.Context.SendActivityAsync(message);
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<DialogTurnResult> AskFullnameDialog(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            var message = "You picked the option to create a new record of you, to register I need you to write your full name";

            var options = new PromptOptions
            {
                Prompt = new Activity { Type = ActivityTypes.Message, Text = $"{message}" }
            };
            return await step.PromptAsync("TextPromptValidator", options, cancellationToken);
        }

        private async Task<DialogTurnResult> GetFullnameDialog(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            var fullname = (string)step.Result;
            step.ActiveDialog.State["fullname"] = fullname.ToUpper();
            return await step.NextAsync();
        }

        private async Task<DialogTurnResult> ListCombinationsDialog(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            List<(string name, string lastname)> combinations = NameHelper.Combinations(step.ActiveDialog.State["fullname"].ToString());

            if (combinations.Count > 1)
            {
                step.ActiveDialog.State["hasCombinations"] = true;

                var message_name = "name:";

                var message_lastname = "last name:";

                List<Choice> choices = new List<Choice>();
                foreach ((string n, string l) in combinations)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"{message_name} [{n}], {message_lastname} [{l}]");
                    choices.Add(new Choice { Value = $"{sb.ToString()}" });
                }

                var message = "Let me know how should I understand your name and last name";
                await step.Context.SendActivityAsync(message);

                PromptOptions options = new PromptOptions
                {
                    Choices = choices,
                    RetryPrompt = new Activity { Type = ActivityTypes.Message, Text = $"{message}" }
                };

                return await step.PromptAsync("ChoicePrompt", options, cancellationToken);
            }
            else
            {
                step.ActiveDialog.State["hasCombinations"] = false;

                step.ActiveDialog.State["name"] = combinations[0].name;
                step.ActiveDialog.State["lastname"] = combinations[0].lastname;

                return await step.NextAsync();
            }
        }

        private async Task<DialogTurnResult> DecomposeNameLastnameDialog(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            bool hasCombinations = (bool)step.ActiveDialog.State["hasCombinations"];

            if (hasCombinations)
            {
                var choiceResult = (FoundChoice)step.Result;
                var choice = choiceResult.Value;

                List<string> fullname = NameHelper.GetFromSquareBrackets(choice);
                step.ActiveDialog.State["name"] = fullname[0];
                step.ActiveDialog.State["lastname"] = fullname[1];
            }

            var message = "You confirmed your name is: {{00}} and your last name is: {{01}}";
            message = message.Replace("{{00}}", $"{step.ActiveDialog.State["name"]}");
            message = message.Replace("{{01}}", $"{step.ActiveDialog.State["lastname"]}");
            await step.Context.SendActivityAsync($"{message}");

            return await step.NextAsync();
        }

        private async Task<DialogTurnResult> EndProfileDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            await accessors.FullnamePreference.SetAsync(step.Context, step.ActiveDialog.State["fullname"].ToString());
            await accessors.NamePreference.SetAsync(step.Context, step.ActiveDialog.State["name"].ToString());
            await accessors.LastnamePreference.SetAsync(step.Context, step.ActiveDialog.State["lastname"].ToString());
            await accessors.UserState.SaveChangesAsync(step.Context, false, cancellationToken);

            //ending dialog
            return await step.EndDialogAsync(step.ActiveDialog.State);
        }
    }
}