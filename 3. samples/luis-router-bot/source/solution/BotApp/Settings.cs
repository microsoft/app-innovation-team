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
        public static string KeyVaultEncryptionKey { get; set; }
        public static string KeyVaultApplicationCode { get; set; }
    }
}