using BotApp;
using FaceClientSDK;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace IBE.Tests.Fixtures
{
    public class SettingsFixture : IDisposable
    {
        public SettingsFixture()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddIniFile("config.ini", optional: false, reloadOnChange: true)
                .Build();

            var MicrosoftAppId = config.GetSection("general:MicrosoftAppId").Value;
            Settings.MicrosoftAppId = MicrosoftAppId;

            var MicrosoftAppPassword = config.GetSection("general:MicrosoftAppPassword").Value;
            Settings.MicrosoftAppPassword = MicrosoftAppPassword;

            var BotVersion = config.GetSection("general:BotVersion").Value;
            Settings.BotVersion = BotVersion;

            var TimeZone = config.GetSection("general:TimeZone").Value;
            Settings.TimeZone = TimeZone;

            var BotConversationStorageConnectionString = config.GetSection("general:BotConversationStorageConnectionString").Value;
            Settings.BotConversationStorageConnectionString = BotConversationStorageConnectionString;

            var BotConversationStorageKey = config.GetSection("general:BotConversationStorageKey").Value;
            Settings.BotConversationStorageKey = BotConversationStorageKey;

            var BotConversationStorageDatabaseId = config.GetSection("general:BotConversationStorageDatabaseId").Value;
            Settings.BotConversationStorageDatabaseId = BotConversationStorageDatabaseId;

            var BotConversationStorageUserCollection = config.GetSection("general:BotConversationStorageUserCollection").Value;
            Settings.BotConversationStorageUserCollection = BotConversationStorageUserCollection;

            var BotConversationStorageConversationCollection = config.GetSection("general:BotConversationStorageConversationCollection").Value;
            Settings.BotConversationStorageConversationCollection = BotConversationStorageConversationCollection;

            var LuisName01 = config.GetSection("general:LuisName01").Value;
            Settings.LuisName01 = LuisName01;

            var LuisAppId01 = config.GetSection("general:LuisAppId01").Value;
            Settings.LuisAppId01 = LuisAppId01;

            var LuisAuthoringKey01 = config.GetSection("general:LuisAuthoringKey01").Value;
            Settings.LuisAuthoringKey01 = LuisAuthoringKey01;

            var LuisEndpoint01 = config.GetSection("general:LuisEndpoint01").Value;
            Settings.LuisEndpoint01 = LuisEndpoint01;

            var QnAName01 = config.GetSection("general:QnAName01").Value;
            Settings.QnAName01 = QnAName01;

            var QnAKbId01 = config.GetSection("general:QnAKbId01").Value;
            Settings.QnAKbId01 = QnAKbId01;

            var QnAEndpointKey01 = config.GetSection("general:QnAEndpointKey01").Value;
            Settings.QnAEndpointKey01 = QnAEndpointKey01;

            var QnAHostname01 = config.GetSection("general:QnAHostname01").Value;
            Settings.QnAHostname01 = QnAHostname01;

            var AzureWebJobsStorage = config.GetSection("general:AzureWebJobsStorage").Value;
            Settings.AzureWebJobsStorage = AzureWebJobsStorage;

            var FaceAPIKey = config.GetSection("general:FaceAPIKey").Value;
            Settings.FaceAPIKey = FaceAPIKey;

            var FaceAPIZone = config.GetSection("general:FaceAPIZone").Value;
            Settings.FaceAPIZone = FaceAPIZone;

            var LargeFaceListId = config.GetSection("general:LargeFaceListId").Value;
            Settings.LargeFaceListId = LargeFaceListId;

            var MongoDBConnectionString = config.GetSection("general:MongoDBConnectionString").Value;
            Settings.MongoDBConnectionString = MongoDBConnectionString;

            var MongoDBDatabaseId = config.GetSection("general:MongoDBDatabaseId").Value;
            Settings.MongoDBDatabaseId = MongoDBDatabaseId;

            var PersonCollection = config.GetSection("general:PersonCollection").Value;
            Settings.PersonCollection = PersonCollection;

            //setting FaceAPISDK
            APIReference.FaceAPIKey = FaceAPIKey;
            APIReference.FaceAPIZone = FaceAPIZone;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }
    }
}