namespace RF.Contracts.Domain.Entities.Blockchain
{
    public class BlockchainInfo
    {
        public string RPCUrl { get; set; }
        public string MasterAddress { get; set; }
        public string MasterPrivateKey { get; set; }
        public string ContractByteCode { get; set; }
        public string ContractABI { get; set; }
    }
}