using FaceClientSDK;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public class IdentifyDialog : ComponentDialog
    {
        public const string dialogId = "IdentifyDialog";
        private BotAccessors accessors = null;
        private PersonDBHelper personDBHelper = null;

        public IdentifyDialog(BotAccessors accessors) : base(dialogId)
        {
            this.personDBHelper = new PersonDBHelper(DBConfiguration.GetMongoDbConnectionInfo());
            this.accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            AddDialog(new WaterfallDialog(dialogId, new WaterfallStep[]
           {
                IntroDialog,
                CheckAskForImageDialog,
                RequestPasscodeDialog,
                ResponsePasscodeDialog,
                SaveInformationIfNewDialog,
                EndIdentifyDialog
           }));

            AddDialog(new TextPrompt("TextPromptValidator", validator));
            AddDialog(new AskForFaceImageDialog(accessors));
            AddDialog(new ProfileDialog(accessors));
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
                if (value != "/create")
                {
                    var message = "";
                    if (value.Length < 8)
                    {
                        message = "Your passcode must be at least 8 characters long, try again";
                        await promptContext.Context.SendActivityAsync($"{message}");
                        return false;
                    }

                    message = "Wait, I'm trying to remember you";
                    await promptContext.Context.SendActivityAsync($"{message}");

                    var hashed = HashHelper.GetSha256Hash(value);

                    string detectedFaceId = await accessors.DetectedFaceIdPreference.GetAsync(promptContext.Context, () => { return string.Empty; });

                    if (string.IsNullOrEmpty(detectedFaceId))
                        throw new System.Exception("there was an issue reading the detected face id preference");

                    List<FaceClientSDK.Domain.Face.FindSimilarResult> similarFaces = await APIReference.Instance.Face.FindSimilarAsync(detectedFaceId, string.Empty, Settings.LargeFaceListId, new string[] { }, 10, "matchPerson");

                    Person person = null;
                    foreach (FaceClientSDK.Domain.Face.FindSimilarResult fs in similarFaces)
                    {
                        List<Person> persons = new List<Person>();
                        persons = await personDBHelper.GetPersonListByFaceAsync(fs.persistedFaceId);
                        person = persons.Find(x => x.PasscodeHash == hashed);

                        if (person != null)
                            break;
                    }

                    if (person == null)
                    {
                        message = "It seems your passcode it's incorrect, if you want to create a new profile type {{00}} or try again with the correct passcode";
                        message = message.Replace("{{00}}", $"/create");
                        await promptContext.Context.SendActivityAsync($"{message}");
                    }
                    else
                    {
                        await accessors.IsNewPreference.SetAsync(promptContext.Context, false);
                        await accessors.FullnamePreference.SetAsync(promptContext.Context, $"{person.Name} {person.Lastname}");
                        await accessors.NamePreference.SetAsync(promptContext.Context, person.Name);
                        await accessors.LastnamePreference.SetAsync(promptContext.Context, person.Lastname);
                        await accessors.UserState.SaveChangesAsync(promptContext.Context, false, cancellationToken);

                        return true;
                    }
                }
                else
                {
                    await accessors.IsNewPreference.SetAsync(promptContext.Context, true);
                    await accessors.UserState.SaveChangesAsync(promptContext.Context, false, cancellationToken);

                    return true;
                }
            }

            return false;
        }

        private async Task<DialogTurnResult> IntroDialog(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            return await step.NextAsync();
        }

        private async Task<DialogTurnResult> CheckAskForImageDialog(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            return await step.BeginDialogAsync(AskForFaceImageDialog.dialogId);
        }

        private async Task<DialogTurnResult> RequestPasscodeDialog(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            var message = "If it's the first time you came here type {{00}} to create a new profile, if you have been registered type your passcode";
            message = message.Replace("{{00}}", $"/create");

            var options = new PromptOptions
            {
                Prompt = new Activity { Type = ActivityTypes.Message, Text = $"{message}" }
            };
            return await step.PromptAsync("TextPromptValidator", options, cancellationToken);
        }

        private async Task<DialogTurnResult> ResponsePasscodeDialog(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            bool isNew = await accessors.IsNewPreference.GetAsync(step.Context, () => { return false; });

            if (isNew)
            {
                return await step.BeginDialogAsync(ProfileDialog.dialogId);
            }
            else
            {
                return await step.NextAsync();
            }
        }

        private async Task<DialogTurnResult> SaveInformationIfNewDialog(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            bool isNew = await accessors.IsNewPreference.GetAsync(step.Context, () => { return false; });
            string name = await accessors.NamePreference.GetAsync(step.Context, () => { return string.Empty; });
            string lastname = await accessors.LastnamePreference.GetAsync(step.Context, () => { return string.Empty; });
            string fullname = await accessors.FullnamePreference.GetAsync(step.Context, () => { return string.Empty; });
            string imageUri = await accessors.ImageUriPreference.GetAsync(step.Context, () => { return string.Empty; });
            string hash = await accessors.HashPreference.GetAsync(step.Context, () => { return string.Empty; });

            if (isNew)
            {
                //add face to list
                FaceClientSDK.Domain.LargeFaceList.AddFaceResult resultFaceToList = await APIReference.Instance.LargeFaceList.AddFaceAsync(Settings.LargeFaceListId, imageUri, fullname, string.Empty);

                //train face model
                await APIReference.Instance.LargeFaceList.TrainAsync(Settings.LargeFaceListId);

                var message = "";

                int timeIntervalInMilliseconds = 1000;
                while (true)
                {
                    message = "Learning to identify your face";
                    await step.Context.SendActivityAsync($"{message}");

                    System.Threading.Tasks.Task.Delay(timeIntervalInMilliseconds).Wait();
                    var status = await APIReference.Instance.LargeFaceList.GetTrainingStatusAsync(Settings.LargeFaceListId);

                    if (status.status == "running")
                    {
                        continue;
                    }
                    else if (status.status == "succeeded")
                    {
                        break;
                    }
                    else
                    {
                        break;
                    }
                }

                message = "Great!, your face has been trained successfully";
                await step.Context.SendActivityAsync($"{message}");

                //adding to mongodb

                var randomUniqueKey = SecurityHelper.GenerateRandomCode();
                var passcodeHash = HashHelper.GetSha256Hash(randomUniqueKey);

                Person person = new Person();
                person.Name = name;
                person.Lastname = lastname;
                person.Hash = hash;
                person.FaceAPIFaceId = resultFaceToList.persistedFaceId;
                person.PasscodeHash = passcodeHash;
                person.CreatedDate = System.DateTime.UtcNow.ToString();

                await personDBHelper.CreatePersonAsync(person);

                message = "Now you will receive a passcode, please keep it safe";
                await step.Context.SendActivityAsync($"{message}");

                message = "{{00}}";
                message = message.Replace("{{00}}", $"{randomUniqueKey}");
                await step.Context.SendActivityAsync($"{message}");

                //delete file from storage
                await StorageHelper.DeleteFileAsync($"{hash}.jpg", "uploads", Settings.AzureWebJobsStorage);

                //is authenticated
                await accessors.IsAuthenticatedPreference.SetAsync(step.Context, true);
            }
            else
            {
                //delete file from storage
                await StorageHelper.DeleteFileAsync($"{hash}.jpg", "uploads", Settings.AzureWebJobsStorage);

                //is authenticated
                await accessors.IsAuthenticatedPreference.SetAsync(step.Context, true);
            }

            return await step.NextAsync();
        }

        private async Task<DialogTurnResult> EndIdentifyDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            //ending dialog
            return await step.EndDialogAsync(step.ActiveDialog.State);
        }
    }
}