using System.Collections.Generic;

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
        public static string QnAKbId { get; set; }
        public static string QnAName { get; set; }
        public static string QnAEndpointKey { get; set; }
        public static string QnAHostname { get; set; }
        public static List<LuisAppRegistration> LuisAppRegistrations { get; set; }
        public static string LuisMiddlewareUrl { get; set; }
        public static string KeyVaultCertificateName { get; set; }
        public static string KeyVaultClientId { get; set; }
        public static string KeyVaultClientSecret { get; set; }
        public static string KeyVaultIdentifier { get; set; }
        public static string KeyVaultEncryptionKey { get; set; }
        public static string KeyVaultApplicationCode { get; set; }
    }
}