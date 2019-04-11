namespace RF.ContentSearch.Api.Domain.Settings
{
    public class ApplicationSettings
    {
        public static string ConnectionString { get; set; }
        public static string DatabaseId { get; set; }
        public static string ContractCollection { get; set; }
        public static string RabbitMQUsername { get; set; }
        public static string RabbitMQPassword { get; set; }
        public static string RabbitMQHostname { get; set; }
        public static int RabbitMQPort { get; set; }
        public static string ContractDeploymentQueueName { get; set; }
        public static string KeyVaultCertificateName { get; set; }
        public static string KeyVaultClientId { get; set; }
        public static string KeyVaultClientSecret { get; set; }
        public static string KeyVaultIdentifier { get; set; }
        public static string KeyVaultEncryptionKey { get; set; }
    }
}