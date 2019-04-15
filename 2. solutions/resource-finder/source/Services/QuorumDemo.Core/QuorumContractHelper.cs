using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Nethereum.Quorum;
using QuorumDemo.Core.Models;
using Nethereum.Web3.Accounts;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.TransactionReceipts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using QuorumDemo.Core.BaseHelpers;

namespace QuorumDemo.Core
{
    public class QuorumContractHelper : BaseHelper
    {
        //static QuorumContractHelper instance = null;
        //static readonly object instancelock = new object();
        //#region Singleton

        //public static QuorumContractHelper Instance
        //{
        //    get
        //    {
        //        if (instance == null)
        //        {
        //            lock (instancelock)
        //            {
        //                instance = new QuorumContractHelper();
        //            }

        //        }

        //        return instance;
        //    }
        //}

        //#endregion

        private Web3Quorum web3 = null;
        private TransactionReceiptPollingService TransactionService;


        public QuorumContractHelper(string RpcURL)
        {
            web3 = new Web3Quorum(RpcURL);
            TransactionService = new TransactionReceiptPollingService(web3.TransactionManager);
        }

        public async Task<TransactionReturnInfo> CreateContractAsync(ContractInfo contractInfo, Account account, object[] inputParams, List<string> PrivateFor = null)
        {
            
            if (web3 == null)
            {
                throw new Exception("web3 handler has not been set - please call SetWeb3Handler First");
            }

            web3.ClearPrivateForRequestParameters();

            if (PrivateFor != null)
            {
                web3.SetPrivateRequestParameters(PrivateFor); 
            }

            //--- get transaction count to set nonce ---// 

            var txCount = await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(account.Address);

            // -- set signor as the account that is sending the transaction --//
            web3.Client.OverridingRequestInterceptor = new AccountTransactionSigningInterceptor(account.PrivateKey, web3.Client);

            try
            {

                var gasDeploy = await web3.Eth.DeployContract.EstimateGasAsync(
                    contractInfo.ContractABI,
                    contractInfo.ContractByteCode,
                    account.Address,
                    inputParams);

                Console.WriteLine("Creating new contract and waiting for address");

                // COULD ALSO USE TransactionService.DeployContractAndWaitForAddress

                var transactionReceipt = await TransactionService.DeployContractAndWaitForReceiptAsync(() =>

                        web3.Eth.DeployContract.SendRequestAsync(
                                contractInfo.ContractABI,
                                contractInfo.ContractByteCode,
                                account.Address, 
                               gas: gasDeploy,
                               gasPrice: new HexBigInteger(0), 
                               value: new HexBigInteger(0), 
                               nonce: txCount,
                               values: inputParams)
                );
                
                Console.WriteLine(transactionReceipt.ContractAddress);
                
                return new TransactionReturnInfo
                {
                    TransactionHash = transactionReceipt.TransactionHash,
                    BlockHash = transactionReceipt.BlockHash,
                    BlockNumber = transactionReceipt.BlockNumber.Value,
                    ContractAddress = transactionReceipt.ContractAddress
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        //public async Task<int> GetTransactionDetailsFromHashAsync(string txHash)
        //{

        //    var res = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);
           
        //    res.

        //}


        public async Task<TransactionReturnInfo> CreateTransactionAsync(string ContractAddress, ContractInfo contractInfo, string FunctionName, Account account, object[] inputParams, List<string> PrivateFor = null)
        {
            if (web3 == null)
            {
                throw new Exception("web3 handler has not been set - please call SetWeb3Handler First");
            }

            web3.ClearPrivateForRequestParameters();

            if (PrivateFor != null)
            {
                web3.SetPrivateRequestParameters(PrivateFor);
            }

            // -- set signor as the account that is sending the transaction --//
            web3.Client.OverridingRequestInterceptor = new AccountTransactionSigningInterceptor(account.PrivateKey, web3.Client);

            var contract = web3.Eth.GetContract(contractInfo.ContractABI, ContractAddress);

            if (contract == null)
            {
                throw new Exception("Could not find contract with ABI at specified address");
            }

            var contractFunction = contract.GetFunction(FunctionName);

            if (contractFunction == null)
            {
                throw new Exception("Could not find function with name " + FunctionName);
            }

            var gasCallFunction = await contractFunction.EstimateGasAsync(
                contractInfo.ContractABI,
                contractInfo.ContractByteCode,
                account.Address,
                inputParams);

            // --- the above call always underestimates gas... WTF to do? --- //

            var realGas = new HexBigInteger(gasCallFunction.Value + 500000);

            try
            {
                var transactionReceipt = await TransactionService.SendRequestAndWaitForReceiptAsync(() =>

                        contractFunction.SendTransactionAsync(
                                account.Address,
                               gas: realGas,
                               gasPrice: new HexBigInteger(0),
                               value: new HexBigInteger(0),
                               functionInput: inputParams)
                );

                if (transactionReceipt != null)
                {
                    Console.WriteLine($"Processed Transaction - txHash: {transactionReceipt.TransactionHash}");

                    return new TransactionReturnInfo
                    {
                        TransactionHash = transactionReceipt.TransactionHash,
                        BlockHash = transactionReceipt.BlockHash,
                        BlockNumber = transactionReceipt.BlockNumber.Value,
                        ContractAddress = transactionReceipt.ContractAddress
                    };

                }

                return null;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }


        public async Task<T> CallContractFunctionAsync<T>(T ReturnType, ContractInfo contractInfo, string contractAddress, string functionName, object[] inputParams = null)
        {
            var contract = web3.Eth.GetContract(contractInfo.ContractABI, contractAddress);
            var function = contract.GetFunction(functionName);
            
            return await function.CallAsync<T>(inputParams);
        }

        public async Task<T> CallContractFunctionAndDeserializeResponseAsync<T>(T FunctionOutputReturnType, ContractInfo contractInfo, string contractAddress, string functionName, object[] inputParams = null)
            where T : new()
        {
            var attributes = typeof(T).GetCustomAttributes(typeof(FunctionOutputAttribute), true);

            if (attributes.Length == 0)
                throw new ArgumentException("T does not have attribute FunctionOutput");

            var contract = web3.Eth.GetContract(contractInfo.ContractABI, contractAddress);
            var function = contract.GetFunction(functionName);
            return await function.CallDeserializingToObjectAsync<T>(inputParams);
        }

    }

}
