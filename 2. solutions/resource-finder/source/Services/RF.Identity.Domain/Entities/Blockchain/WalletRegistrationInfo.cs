namespace RF.Identity.Domain.Entities.Blockchain
{
    public class WalletRegistrationInfo
    {
        public string address { get; set; }
        public string password { get; set; }
        public string privateKey { get; set; }
        public string mnemonic { get; set; }
    }
}