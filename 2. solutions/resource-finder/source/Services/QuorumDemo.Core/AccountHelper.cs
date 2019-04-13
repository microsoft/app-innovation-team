using System;
using System.Threading.Tasks;
using Nethereum.Quorum;
using QuorumDemo.Core.Models;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3.Accounts;
using System.Net.NetworkInformation;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Nethereum.KeyStore;

namespace QuorumDemo.Core
{
    public static class AccountHelper
    {
        public static Account DecryptAccount(string accountJsonFile, string passWord)
        {
            //using the simple key store service
            var service = new KeyStoreService();
            //decrypt the private key
            var key = service.DecryptKeyStoreFromJson(passWord, accountJsonFile);

            return new Nethereum.Web3.Accounts.Account(key);

        }
    }
}
