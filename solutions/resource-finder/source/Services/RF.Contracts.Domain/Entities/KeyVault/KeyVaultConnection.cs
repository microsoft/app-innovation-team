namespace RF.Contracts.Domain.Entities.KeyVault
{
    public class KeyVaultConnectionInfo
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string KeyVaultIdentifier { get; set; }
        public string CertificateName { get; set; }
    }
}