namespace RF.ContractDeployment.App
{
    public class Settings
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
        public static string BlockchainRPCUrl { get; set; }
        public static string BlockchainMasterAddress { get; set; }
        public static string BlockchainMasterPrivateKey { get; set; }
        public static string BlockchainContractABI { get; set; }
        public static string BlockchainContractByteCode { get; set; }
    }
}