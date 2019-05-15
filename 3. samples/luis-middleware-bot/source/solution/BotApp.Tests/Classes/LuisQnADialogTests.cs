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
    public class LuisQnADialogTests : IClassFixture<SettingsFixture>
    {
        private SettingsFixture settingsFixture = null;

        public LuisQnADialogTests(SettingsFixture fixture)
        {
            settingsFixture = fixture;
        }

        [Fact]
        public async void AskForATopicTest()
        {
            var storage = new MemoryStorage();

            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            KeyVaultConnectionInfo keyVaultConnectionInfo = new KeyVaultConnectionInfo()
            {
                CertificateName = Settings.KeyVaultCertificateName,
                ClientId = Settings.KeyVaultClientId,
                ClientSecret = Settings.KeyVaultClientSecret,
                KeyVaultIdentifier = Settings.KeyVaultIdentifier
            };

            using (KeyVaultHelper keyVaultHelper = new KeyVaultHelper(keyVaultConnectionInfo))
            {
                var encryptionkey = keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultEncryptionKey).Result;
                var appcode = keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultApplicationCode).Result;
                Startup.EncryptedKey = NETCore.Encrypt.EncryptProvider.AESEncrypt(appcode, encryptionkey);
            }

            var luisServices = new Dictionary<string, LuisRecognizer>();

            var app = new LuisApplication(TestSettings.LuisAppId01, TestSettings.LuisAuthoringKey01, TestSettings.LuisEndpoint01);
            var recognizer = new LuisRecognizer(app);
            luisServices.Add(TestSettings.LuisName01, recognizer);

            app = new LuisApplication(TestSettings.LuisAppId02, TestSettings.LuisAuthoringKey02, TestSettings.LuisEndpoint02);
            recognizer = new LuisRecognizer(app);
            luisServices.Add(TestSettings.LuisName02, recognizer);

            var qnaEndpoint = new QnAMakerEndpoint()
            {
                KnowledgeBaseId = Settings.QnAKbId,
                EndpointKey = Settings.QnAEndpointKey,
                Host = Settings.QnAHostname,
            };

            var qnaOptions = new QnAMakerOptions
            {
                ScoreThreshold = 0.3F
            };

            var qnaServices = new Dictionary<string, QnAMaker>();
            var qnaMaker = new QnAMaker(qnaEndpoint, qnaOptions);
            qnaServices.Add(Settings.QnAName, qnaMaker);

            var accessors = new BotAccessors(new LoggerFactory(), conversationState, userState, luisServices, qnaServices)
            {
                ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                AskForExamplePreference = conversationState.CreateProperty<bool>("AskForExamplePreference"),
                IsAuthenticatedPreference = userState.CreateProperty<bool>("IsAuthenticatedPreference")
            };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await accessors.ConversationDialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(accessors.ConversationDialogState);
                dialogs.Add(new LuisQnADialog(accessors));

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync(LuisQnADialog.dialogId, null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    //no additional send activities.
                }
            })
            .Send("")
            .AssertReply("What topic would you like to know more about?")
            .Send("what do i need to consider for an interview?")
            .AssertReply("In interviews, your job is to convince a recruiter that you have the skills, knowledge and experience for the job. Show motivation and convince a recruiter that you fit the organization's culture and job description, and you get that much closer to an offer.")
            .StartTestAsync();
        }

        [Fact]
        public async void AskForATopicSampleTest()
        {
            var storage = new MemoryStorage();

            var userState = new UserState(storage);
            var conversationState = new ConversationState(storage);

            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(conversationState));

            KeyVaultConnectionInfo keyVaultConnectionInfo = new KeyVaultConnectionInfo()
            {
                CertificateName = Settings.KeyVaultCertificateName,
                ClientId = Settings.KeyVaultClientId,
                ClientSecret = Settings.KeyVaultClientSecret,
                KeyVaultIdentifier = Settings.KeyVaultIdentifier
            };

            using (KeyVaultHelper keyVaultHelper = new KeyVaultHelper(keyVaultConnectionInfo))
            {
                var encryptionkey = keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultEncryptionKey).Result;
                var appcode = keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultApplicationCode).Result;
                Startup.EncryptedKey = NETCore.Encrypt.EncryptProvider.AESEncrypt(appcode, encryptionkey);
            }

            var luisServices = new Dictionary<string, LuisRecognizer>();

            var app = new LuisApplication(TestSettings.LuisAppId01, TestSettings.LuisAuthoringKey01, TestSettings.LuisEndpoint01);
            var recognizer = new LuisRecognizer(app);
            luisServices.Add(TestSettings.LuisName01, recognizer);

            app = new LuisApplication(TestSettings.LuisAppId02, TestSettings.LuisAuthoringKey02, TestSettings.LuisEndpoint02);
            recognizer = new LuisRecognizer(app);
            luisServices.Add(TestSettings.LuisName02, recognizer);

            var qnaEndpoint = new QnAMakerEndpoint()
            {
                KnowledgeBaseId = Settings.QnAKbId,
                EndpointKey = Settings.QnAEndpointKey,
                Host = Settings.QnAHostname,
            };

            var qnaOptions = new QnAMakerOptions
            {
                ScoreThreshold = 0.3F
            };

            var qnaServices = new Dictionary<string, QnAMaker>();
            var qnaMaker = new QnAMaker(qnaEndpoint, qnaOptions);
            qnaServices.Add(Settings.QnAName, qnaMaker);

            var accessors = new BotAccessors(new LoggerFactory(), conversationState, userState, luisServices, qnaServices)
            {
                ConversationDialogState = conversationState.CreateProperty<DialogState>("DialogState"),
                AskForExamplePreference = conversationState.CreateProperty<bool>("AskForExamplePreference"),
                IsAuthenticatedPreference = userState.CreateProperty<bool>("IsAuthenticatedPreference")
            };

            List<MediaUrl> mediaList = new List<MediaUrl>();
            mediaList.Add(new MediaUrl("https://www.youtube.com/watch?v=CmTSY9oO3dw"));

            VideoCard videoCard = new VideoCard
            {
                Title = "Interview Sample",
                Text = "Each interview takes on a life of its own, but there are certain standard questions that arise. By reviewing them in advance, you can arrive confident and ready to articulate your skills and qualifications. Take a look at the sample questions here, and then bolster them with those specific to your goals and the organization. Both your answers to the interviewer's questions and those you post to them can provide a mechanism by which to communicate your qualifications.",
                Autostart = false,
                Media = mediaList
            };

            Activity activity = new Activity
            {
                Type = ActivityTypes.Message,
                Attachments = new List<Attachment> { videoCard.ToAttachment() }
            };

            await new TestFlow(adapter, async (turnContext, cancellationToken) =>
            {
                var state = await accessors.ConversationDialogState.GetAsync(turnContext, () => new DialogState());
                var dialogs = new DialogSet(accessors.ConversationDialogState);
                dialogs.Add(new LuisQnADialog(accessors));

                var dc = await dialogs.CreateContextAsync(turnContext, cancellationToken);

                var results = await dc.ContinueDialogAsync(cancellationToken);
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dc.BeginDialogAsync(LuisQnADialog.dialogId, null, cancellationToken);
                }
                else if (results.Status == DialogTurnStatus.Complete)
                {
                    //no additional send activities.
                }
            })
            .Send("")
            .AssertReply("What topic would you like to know more about?")
            .Send("i would like to see a sample about the considerations for a human resources interview")
            .AssertReply((ac) =>
            {
                if (ac.AsMessageActivity().Attachments != null)
                {
                    var contentType = ac.AsMessageActivity().Attachments[0].ContentType;
                    Assert.Equal("application/vnd.microsoft.card.video", contentType);
                }
                else
                    Assert.NotNull(ac.AsMessageActivity().Attachments);
            })
            .StartTestAsync();
        }
    }
}