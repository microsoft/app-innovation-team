using FaceClientSDK;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BotApp
{
    public class AskForFaceImageDialog : ComponentDialog
    {
        public const string dialogId = "AskForFaceImageDialog";
        private BotAccessors accessors = null;

        public AskForFaceImageDialog(BotAccessors accessors) : base(dialogId)
        {
            this.accessors = accessors ?? throw new ArgumentNullException(nameof(accessors));

            AddDialog(new WaterfallDialog(dialogId, new WaterfallStep[]
            {
                RequestImageDialog,
                EndAskForFaceImageDialog
            }));

            AddDialog(new AttachmentPrompt("AttachmentPromptValidator", validator));
        }

        private async Task<bool> validator(PromptValidatorContext<IList<Attachment>> promptContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (promptContext.Recognized.Value == null)
            {
                var message = $"Sorry, but I'm expecting an attachment image file, try again with other picture";
                await promptContext.Context.SendActivityAsync($"{message}");
            }
            else
            {
                if (promptContext.Recognized.Value.Count > 0)
                {
                    if (promptContext.Recognized.Value[0].ContentType != "image/jpeg")
                    {
                        var message = $"Sorry, I just can understand jpeg files, try again with other picture";
                        await promptContext.Context.SendActivityAsync($"{message}");
                    }
                    else
                    {
                        var hash = string.Empty;

                        try
                        {
                            byte[] imageBytes = null;
                            using (var httpClient = new HttpClient())
                                imageBytes = await httpClient.GetByteArrayAsync(promptContext.Recognized.Value[0].ContentUrl);

                            var base64 = Convert.ToBase64String(imageBytes);
                            hash = HashHelper.GetSha256Hash(base64);

                            var imageUri = string.Empty;
                            int retries = 0;

                            while (imageUri == string.Empty)
                            {
                                var stream = new System.IO.MemoryStream(imageBytes);
                                imageUri = await StorageHelper.UploadFileAsync(stream, $"{hash}.jpg", "uploads", Settings.AzureWebJobsStorage, "image/jpeg");

                                retries++;
                                if (retries > 3)
                                    break;
                            }

                            if (imageUri == string.Empty)
                                throw new Exception("there is no image uri");

                            List<FaceClientSDK.Domain.Face.DetectResult> list = await APIReference.Instance.Face.DetectAsync(imageUri, "age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise", true, true);
                            bool isValid = true;

                            if (list.Count == 0)
                            {
                                isValid = false;
                                //delete file from storage
                                await StorageHelper.DeleteFileAsync($"{hash}.jpg", "uploads", Settings.AzureWebJobsStorage);

                                var message = $"Sorry, I can't see your face well, try again with other picture";
                                await promptContext.Context.SendActivityAsync($"{message}");
                            }

                            if (list.Count > 1)
                            {
                                isValid = false;
                                //delete file from storage
                                await StorageHelper.DeleteFileAsync($"{hash}.jpg", "uploads", Settings.AzureWebJobsStorage);

                                var message = $"Sorry, I see there are multiple persons in the picture, try again with other picture";
                                await promptContext.Context.SendActivityAsync($"{message}");
                            }

                            if (isValid)
                            {
                                await accessors.DetectedFaceIdPreference.SetAsync(promptContext.Context, list.First().faceId);
                                await accessors.ImageUriPreference.SetAsync(promptContext.Context, imageUri);
                                await accessors.HashPreference.SetAsync(promptContext.Context, hash);
                                await accessors.UserState.SaveChangesAsync(promptContext.Context, false, cancellationToken);

                                return true;
                            }
                        }
                        catch
                        {
                            //delete file from storage
                            await StorageHelper.DeleteFileAsync($"{hash}.jpg", "uploads", Settings.AzureWebJobsStorage);

                            var message = $"Sorry, there was an exception, try again";
                            await promptContext.Context.SendActivityAsync($"{message}");
                        }
                    }
                }
            }

            return false;
        }

        private async Task<DialogTurnResult> RequestImageDialog(WaterfallStepContext step, CancellationToken cancellationToken)
        {
            var message = $"Let's go to identify you, send me a picture of you";

            var options = new PromptOptions
            {
                Prompt = new Activity { Type = ActivityTypes.Message, Text = $"{message}" }
            };
            return await step.PromptAsync("AttachmentPromptValidator", options, cancellationToken);
        }

        private async Task<DialogTurnResult> EndAskForFaceImageDialog(WaterfallStepContext step, CancellationToken cancellationToken = default(CancellationToken))
        {
            //ending dialog
            return await step.EndDialogAsync(step.ActiveDialog.State);
        }
    }
}