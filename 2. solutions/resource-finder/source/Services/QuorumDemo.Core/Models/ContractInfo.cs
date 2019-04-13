using System;
using System.Security.Cryptography.X509Certificates;
namespace QuorumDemo.Core.Models
{
    public class ContractInfo
    {
        public string ContractABI { get; set; }
        public string ContractByteCode { get; set; }
    }
}
