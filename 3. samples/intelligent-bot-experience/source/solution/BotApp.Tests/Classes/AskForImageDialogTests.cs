using IBE.Tests.Fixtures;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Xunit;

namespace BotApp.Tests.Classes
{
    public class AskForImageDialogTests : IClassFixture<SettingsFixture>
    {
        private SettingsFixture settingsFixture = null;

        public AskForImageDialogTests(SettingsFixture fixture)
        {
            settingsFixture = fixture;
        }

        [Fact]
        public async void SendTextInsteadImageTest()
        {
            var storage = new MemoryStorage();

            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            var accessors = new BotAccessors(new LoggerFactory(), conversationState, userState, new Dictionary<string, LuisRecognizer>(), new Dictionary<string, QnAMaker>())
            {
                ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                AskForExamplePreference = conversationState.CreateProperty<bool>("AskForExamplePreference"),
                DetectedFaceIdPreference = conversationState.CreateProperty<string>("DetectedFaceIdPreference"),
                ImageUriPreference = conversationState.CreateProperty<string>("ImageUriPreference"),
                HashPreference = conversationState.CreateProperty<string>("HashPreference"),
                IsNewPreference = conversationState.CreateProperty<bool>("IsNewPreference"),
                FullnamePreference = userState.CreateProperty<string>("FullnamePreference"),
                NamePreference = userState.CreateProperty<string>("NamePreference"),
                LastnamePreference = userState.CreateProperty<string>("LastnamePreference"),
                IsAuthenticatedPreference = userState.CreateProperty<bool>("IsAuthenticatedPreference")
            };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await accessors.ConversationDialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(accessors.ConversationDialogState);
                dialogs.Add(new AskForFaceImageDialog(accessors));

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync(AskForFaceImageDialog.dialogId, null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    //no additional send activities.
                }
            })
            .Send("")
            .AssertReply("Let's go to identify you, send me a picture of you")
            .Send("hello")
            .AssertReply("Sorry, but I'm expecting an attachment image file, try again with other picture")
            .StartTestAsync();
        }

        [Fact]
        public async void SendTxtFileTest()
        {
            var storage = new MemoryStorage();

            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            var accessors = new BotAccessors(new LoggerFactory(), conversationState, userState, new Dictionary<string, LuisRecognizer>(), new Dictionary<string, QnAMaker>())
            {
                ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                AskForExamplePreference = conversationState.CreateProperty<bool>("AskForExamplePreference"),
                DetectedFaceIdPreference = conversationState.CreateProperty<string>("DetectedFaceIdPreference"),
                ImageUriPreference = conversationState.CreateProperty<string>("ImageUriPreference"),
                HashPreference = conversationState.CreateProperty<string>("HashPreference"),
                IsNewPreference = conversationState.CreateProperty<bool>("IsNewPreference"),
                FullnamePreference = userState.CreateProperty<string>("FullnamePreference"),
                NamePreference = userState.CreateProperty<string>("NamePreference"),
                LastnamePreference = userState.CreateProperty<string>("LastnamePreference"),
                IsAuthenticatedPreference = userState.CreateProperty<bool>("IsAuthenticatedPreference")
            };

            var attachment = new Attachment
            {
                Content = "my txt file",
                ContentType = "text/plain"
            };

            Activity activity = new Activity
            {
                Type = ActivityTypes.Message,
                Attachments = new List<Attachment> { attachment }
            };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await accessors.ConversationDialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(accessors.ConversationDialogState);
                dialogs.Add(new AskForFaceImageDialog(accessors));

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync(AskForFaceImageDialog.dialogId, null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    //no additional send activities.
                }
            })
            .Send("")
            .AssertReply("Let's go to identify you, send me a picture of you")
            .Send(activity)
            .AssertReply("Sorry, I just can understand jpeg files, try again with other picture")
            .StartTestAsync();
        }

        [Fact]
        public async void SendNoFaceImageTest()
        {
            var storage = new MemoryStorage();

            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            var accessors = new BotAccessors(new LoggerFactory(), conversationState, userState, new Dictionary<string, LuisRecognizer>(), new Dictionary<string, QnAMaker>())
            {
                ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                AskForExamplePreference = conversationState.CreateProperty<bool>("AskForExamplePreference"),
                DetectedFaceIdPreference = conversationState.CreateProperty<string>("DetectedFaceIdPreference"),
                ImageUriPreference = conversationState.CreateProperty<string>("ImageUriPreference"),
                HashPreference = conversationState.CreateProperty<string>("HashPreference"),
                IsNewPreference = conversationState.CreateProperty<bool>("IsNewPreference"),
                FullnamePreference = userState.CreateProperty<string>("FullnamePreference"),
                NamePreference = userState.CreateProperty<string>("NamePreference"),
                LastnamePreference = userState.CreateProperty<string>("LastnamePreference"),
                IsAuthenticatedPreference = userState.CreateProperty<bool>("IsAuthenticatedPreference")
            };

            var attachment = new Attachment
            {
                ContentUrl = "https://images.pexels.com/photos/1020315/pexels-photo-1020315.jpeg?auto=compress&cs=tinysrgb&dpr=2&h=750&w=1260",
                ContentType = "image/jpeg"
            };

            Activity activity = new Activity
            {
                Type = ActivityTypes.Message,
                Attachments = new List<Attachment> { attachment }
            };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await accessors.ConversationDialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(accessors.ConversationDialogState);
                dialogs.Add(new AskForFaceImageDialog(accessors));

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync(AskForFaceImageDialog.dialogId, null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    //no additional send activities.
                }
            })
            .Send("")
            .AssertReply("Let's go to identify you, send me a picture of you")
            .Send(activity)
            .AssertReply("Sorry, I can't see your face well, try again with other picture")
            .StartTestAsync();
        }

        [Fact]
        public async void SendMultipleFacesImageTest()
        {
            var storage = new MemoryStorage();

            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            var accessors = new BotAccessors(new LoggerFactory(), conversationState, userState, new Dictionary<string, LuisRecognizer>(), new Dictionary<string, QnAMaker>())
            {
                ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                AskForExamplePreference = conversationState.CreateProperty<bool>("AskForExamplePreference"),
                DetectedFaceIdPreference = conversationState.CreateProperty<string>("DetectedFaceIdPreference"),
                ImageUriPreference = conversationState.CreateProperty<string>("ImageUriPreference"),
                HashPreference = conversationState.CreateProperty<string>("HashPreference"),
                IsNewPreference = conversationState.CreateProperty<bool>("IsNewPreference"),
                FullnamePreference = userState.CreateProperty<string>("FullnamePreference"),
                NamePreference = userState.CreateProperty<string>("NamePreference"),
                LastnamePreference = userState.CreateProperty<string>("LastnamePreference"),
                IsAuthenticatedPreference = userState.CreateProperty<bool>("IsAuthenticatedPreference")
            };

            var attachment = new Attachment
            {
                ContentUrl = "https://www.jcpportraits.com/sites/jcpportraits.com/files/portrait/1712/1_182-456_FamilyGallery14.jpg",
                ContentType = "image/jpeg"
            };

            Activity activity = new Activity
            {
                Type = ActivityTypes.Message,
                Attachments = new List<Attachment> { attachment }
            };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await accessors.ConversationDialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(accessors.ConversationDialogState);
                dialogs.Add(new AskForFaceImageDialog(accessors));

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync(AskForFaceImageDialog.dialogId, null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    //no additional send activities.
                }
            })
            .Send("")
            .AssertReply("Let's go to identify you, send me a picture of you")
            .Send(activity)
            .AssertReply("Sorry, I see there are multiple persons in the picture, try again with other picture")
            .StartTestAsync();
        }

        [Fact]
        public async void SendValidImageTest()
        {
            var storage = new MemoryStorage();

            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            var accessors = new BotAccessors(new LoggerFactory(), conversationState, userState, new Dictionary<string, LuisRecognizer>(), new Dictionary<string, QnAMaker>())
            {
                ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                AskForExamplePreference = conversationState.CreateProperty<bool>("AskForExamplePreference"),
                DetectedFaceIdPreference = conversationState.CreateProperty<string>("DetectedFaceIdPreference"),
                ImageUriPreference = conversationState.CreateProperty<string>("ImageUriPreference"),
                HashPreference = conversationState.CreateProperty<string>("HashPreference"),
                IsNewPreference = conversationState.CreateProperty<bool>("IsNewPreference"),
                FullnamePreference = userState.CreateProperty<string>("FullnamePreference"),
                NamePreference = userState.CreateProperty<string>("NamePreference"),
                LastnamePreference = userState.CreateProperty<string>("LastnamePreference"),
                IsAuthenticatedPreference = userState.CreateProperty<bool>("IsAuthenticatedPreference")
            };

            var attachment = new Attachment
            {
                ContentUrl = "https://images.pexels.com/photos/415829/pexels-photo-415829.jpeg?auto=compress&cs=tinysrgb&dpr=2&h=750&w=1260",
                ContentType = "image/jpeg"
            };

            Activity activity = new Activity
            {
                Type = ActivityTypes.Message,
                Attachments = new List<Attachment> { attachment }
            };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await accessors.ConversationDialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(accessors.ConversationDialogState);
                dialogs.Add(new AskForFaceImageDialog(accessors));

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync(AskForFaceImageDialog.dialogId, null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    string detectedFaceIdPreference = await accessors.DetectedFaceIdPreference.GetAsync(turnContext, () => { return string.Empty; });
                    string imageUriPreference = await accessors.ImageUriPreference.GetAsync(turnContext, () => { return string.Empty; });
                    string hashPreference = await accessors.HashPreference.GetAsync(turnContext, () => { return string.Empty; });

                    //delete file from storage
                    await StorageHelper.DeleteFileAsync($"{hashPreference}.jpg", "uploads", Settings.AzureWebJobsStorage);

                    Assert.True(detectedFaceIdPreference.Length > 0);
                    Assert.True(imageUriPreference.Length > 0);
                    Assert.True(hashPreference.Length > 0);
                }
            })
            .Send("")
            .AssertReply("Let's go to identify you, send me a picture of you")
            .Send(activity)
            .StartTestAsync();
        }

        [Fact]
        public async void ErrorExceptionProcessingImageTest()
        {
            var storage = new MemoryStorage();

            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            var accessors = new BotAccessors(new LoggerFactory(), conversationState, userState, new Dictionary<string, LuisRecognizer>(), new Dictionary<string, QnAMaker>())
            {
                ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                AskForExamplePreference = conversationState.CreateProperty<bool>("AskForExamplePreference"),
                DetectedFaceIdPreference = conversationState.CreateProperty<string>("DetectedFaceIdPreference"),
                ImageUriPreference = conversationState.CreateProperty<string>("ImageUriPreference"),
                HashPreference = conversationState.CreateProperty<string>("HashPreference"),
                IsNewPreference = conversationState.CreateProperty<bool>("IsNewPreference"),
                FullnamePreference = userState.CreateProperty<string>("FullnamePreference"),
                NamePreference = userState.CreateProperty<string>("NamePreference"),
                LastnamePreference = userState.CreateProperty<string>("LastnamePreference"),
                IsAuthenticatedPreference = userState.CreateProperty<bool>("IsAuthenticatedPreference")
            };

            var attachment = new Attachment
            {
                ContentUrl = "incorrect url",
                ContentType = "image/jpeg"
            };

            Activity activity = new Activity
            {
                Type = ActivityTypes.Message,
                Attachments = new List<Attachment> { attachment }
            };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await accessors.ConversationDialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(accessors.ConversationDialogState);
                dialogs.Add(new AskForFaceImageDialog(accessors));

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync(AskForFaceImageDialog.dialogId, null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    //no additional send activities.
                }
            })
            .Send("")
            .AssertReply("Let's go to identify you, send me a picture of you")
            .Send(activity)
            .AssertReply("Sorry, there was an exception, try again")
            .StartTestAsync();
        }
    }
}