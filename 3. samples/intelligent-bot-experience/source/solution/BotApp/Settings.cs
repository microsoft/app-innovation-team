namespace BotApp
{
    public class Settings
    {
        public static string MicrosoftAppId { get; set; }
        public static string MicrosoftAppPassword { get; set; }
        public static string BotConversationStorageConnectionString { get; set; }
        public static string BotConversationStorageKey { get; set; }
        public static string BotConversationStorageDatabaseId { get; set; }
        public static string BotConversationStorageUserCollection { get; set; }
        public static string BotConversationStorageConversationCollection { get; set; }
        public static string LuisAppId01 { get; set; }
        public static string LuisName01 { get; set; }
        public static string LuisAuthoringKey01 { get; set; }
        public static string LuisEndpoint01 { get; set; }
        public static string QnAKbId01 { get; set; }
        public static string QnAName01 { get; set; }
        public static string QnAEndpointKey01 { get; set; }
        public static string QnAHostname01 { get; set; }
        public static string AzureWebJobsStorage { get; set; }
        public static string FaceAPIKey { get; set; }
        public static string FaceAPIZone { get; set; }
        public static string LargeFaceListId { get; set; }
        public static string MongoDBConnectionString { get; set; }
        public static string MongoDBDatabaseId { get; set; }
        public static string PersonCollection { get; set; }
    }
}