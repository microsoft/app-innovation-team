using System;
using System.Numerics;
namespace QuorumDemo.Core.Models
{
    public class TransactionReturnInfo
    {
        public string TransactionHash { get; set; }
        public BigInteger BlockNumber { get; set; }
        public string BlockHash { get; set; }
        public string ContractAddress { get; set; }
    }
}
