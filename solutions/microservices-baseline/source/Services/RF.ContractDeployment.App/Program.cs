using Microsoft.Extensions.Configuration;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RF.ContractDeployment.App.Helpers.Data;
using RF.ContractDeployment.App.Helpers.KeyVault;
using RF.Contracts.Domain.Entities.Blockchain;
using RF.Contracts.Domain.Entities.ContractDeployment_App;
using RF.Contracts.Domain.Entities.Data;
using RF.Contracts.Domain.Entities.KeyVault;
using RF.Contracts.Domain.Entities.Queue;
using RF.Contracts.Domain.Enums;
using RF.Contracts.Domain.Enums.ContractDeployment_App;
using RF.Contracts.Domain.Exceptions;
using System;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RF.ContractDeployment.App
{
    internal class Program
    {
        // AutoResetEvent to signal when to exit the application
        private static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);

        private static MongoDBConnectionInfo mongoDBConnectionInfo = null;
        private static KeyVaultConnectionInfo keyVaultConnectionInfo = null;
        private static BlockchainInfo blockchainInfo = null;
        private static string secret = string.Empty;
        private static Account account = null;
        private static Web3 web3 = null;

        private static void Init()
        {
            var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true, true)
            .AddEnvironmentVariables();

            IConfigurationRoot Configuration = builder.Build();

            // Retrieve configuration from sections
            Settings.ConnectionString = Configuration.GetSection("ConnectionString")?.Value;
            Settings.DatabaseId = Configuration.GetSection("DatabaseId")?.Value;
            Settings.ContractCollection = Configuration.GetSection("ContractCollection")?.Value;
            Settings.RabbitMQUsername = Configuration.GetSection("RabbitMQUsername")?.Value;
            Settings.RabbitMQPassword = Configuration.GetSection("RabbitMQPassword")?.Value;
            Settings.RabbitMQHostname = Configuration.GetSection("RabbitMQHostname")?.Value;
            Settings.RabbitMQPort = Convert.ToInt16(Configuration.GetSection("RabbitMQPort")?.Value);
            Settings.ContractDeploymentQueueName = Configuration.GetSection("ContractDeploymentQueueName")?.Value;
            Settings.KeyVaultCertificateName = Configuration.GetSection("KeyVaultCertificateName")?.Value;
            Settings.KeyVaultClientId = Configuration.GetSection("KeyVaultClientId")?.Value;
            Settings.KeyVaultClientSecret = Configuration.GetSection("KeyVaultClientSecret")?.Value;
            Settings.KeyVaultIdentifier = Configuration.GetSection("KeyVaultIdentifier")?.Value;
            Settings.KeyVaultEncryptionKey = Configuration.GetSection("KeyVaultEncryptionKey")?.Value;
            Settings.BlockchainRPCUrl = Configuration.GetSection("BlockchainRPCUrl")?.Value;
            Settings.BlockchainMasterAddress = Configuration.GetSection("BlockchainMasterAddress")?.Value;
            Settings.BlockchainMasterPrivateKey = Configuration.GetSection("BlockchainMasterPrivateKey")?.Value;
            Settings.BlockchainContractABI = Configuration.GetSection("BlockchainContractABI")?.Value;
            Settings.BlockchainContractByteCode = Configuration.GetSection("BlockchainContractByteCode")?.Value;

            mongoDBConnectionInfo = new MongoDBConnectionInfo()
            {
                ConnectionString = Settings.ConnectionString,
                DatabaseId = Settings.DatabaseId,
                ContractCollection = Settings.ContractCollection
            };

            keyVaultConnectionInfo = new KeyVaultConnectionInfo()
            {
                CertificateName = Settings.KeyVaultCertificateName,
                ClientId = Settings.KeyVaultClientId,
                ClientSecret = Settings.KeyVaultClientSecret,
                KeyVaultIdentifier = Settings.KeyVaultIdentifier
            };

            blockchainInfo = new BlockchainInfo()
            {
                RPCUrl = Settings.BlockchainRPCUrl,
                MasterAddress = Settings.BlockchainMasterAddress,
                MasterPrivateKey = Settings.BlockchainMasterPrivateKey,
                ContractABI = Settings.BlockchainContractABI,
                ContractByteCode = Settings.BlockchainContractByteCode
            };

            using (KeyVaultHelper keyVaultHelper = new KeyVaultHelper(keyVaultConnectionInfo))
            {
                secret = keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultEncryptionKey).Result;
            }

            account = new Account(blockchainInfo.MasterPrivateKey);
            web3 = new Web3(account, new RpcClient(new Uri(blockchainInfo.RPCUrl), null, null, new HttpClientHandler() { MaxConnectionsPerServer = 10, UseProxy = false }));
        }

        private static void Main(string[] args)
        {
            Task.Run(() =>
            {
                try
                {
                    // initialize settings
                    Init();

                    Console.WriteLine($"Take it easy, the console will display important messages, actually, it's running!! :)");

                    ConnectionFactory factory = new ConnectionFactory();
                    factory.UserName = Settings.RabbitMQUsername;
                    factory.Password = Settings.RabbitMQPassword;
                    factory.HostName = Settings.RabbitMQHostname;
                    factory.Port = Settings.RabbitMQPort;
                    factory.RequestedHeartbeat = 60;
                    factory.DispatchConsumersAsync = true;

                    var connection = factory.CreateConnection();
                    var channel = connection.CreateModel();

                    channel.QueueDeclare(queue: Settings.ContractDeploymentQueueName,
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.Received += async (model, ea) =>
                    {
                        ContractDeploymentResult result = new ContractDeploymentResult
                        {
                            IsSucceded = true,
                            ResultId = (int)ContractDeploymentResultEnum.Success
                        };

                        // forced-to-disposal
                        TransactionReceipt receipt = null;
                        Contract contract = null;
                        ContractDeploymentDataHelper contractDeploymentDataHelper = null;

                        try
                        {
                            byte[] body = ea.Body;

                            var message = Encoding.UTF8.GetString(body);

                            var decrypted = string.Empty;
                            decrypted = NETCore.Encrypt.EncryptProvider.AESDecrypt(message, secret);

                            var obj_decrypted = JsonConvert.DeserializeObject<ContractDeploymentMessage>(decrypted);

                            Console.WriteLine($">> Contract ABI: {blockchainInfo.ContractABI}");
                            Console.WriteLine($">> Contract ByteCode: {blockchainInfo.ContractByteCode}");
                            Console.WriteLine($">> Master Address: {blockchainInfo.MasterAddress}");

                            var gasDeploy = await web3.Eth.DeployContract.EstimateGasAsync(blockchainInfo.ContractABI, blockchainInfo.ContractByteCode, blockchainInfo.MasterAddress, new object[] { });
                            Console.WriteLine($">> Deploying contract using: {gasDeploy.Value} gas");

                            var transaction_hash = await web3.Eth.DeployContract.SendRequestAsync(blockchainInfo.ContractByteCode, blockchainInfo.MasterAddress, gasDeploy, new HexBigInteger(0));
                            Console.WriteLine($">> Transaction hash processed: {transaction_hash}");

                            contract = new Contract()
                            {
                                name = obj_decrypted.name,
                                description = obj_decrypted.description,
                                address = string.Empty,
                                status = string.Empty,
                                transaction_hash = transaction_hash
                            };

                            contractDeploymentDataHelper = new ContractDeploymentDataHelper(mongoDBConnectionInfo);

                            await contractDeploymentDataHelper.RegisterContractAsync(contract);
                            Console.WriteLine($">> Contract registered successfully in database");

                            receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaction_hash);

                            while (receipt == null)
                            {
                                Console.WriteLine($">> Receipt not ready for contract transaction hash: {transaction_hash}");
                                Thread.Sleep(1000);
                                receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transaction_hash);
                            }

                            if (receipt.Status.Value != null)
                            {
                                var status = StatusString(receipt.Status.Value);

                                contractDeploymentDataHelper = new ContractDeploymentDataHelper(mongoDBConnectionInfo);
                                contract = contractDeploymentDataHelper.GetContract(transaction_hash);

                                contract.address = receipt.ContractAddress;
                                contract.status = status;
                                await contractDeploymentDataHelper.UpdateContractAsync(contract);
                                Console.WriteLine($">> Contract updated successfully: {transaction_hash}");

                                channel.BasicAck(ea.DeliveryTag, false);
                                Console.WriteLine($">> Acknowledgement completed, delivery tag: {ea.DeliveryTag}");
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex is BusinessException)
                            {
                                result.IsSucceded = false;
                                result.ResultId = ((BusinessException)ex).ResultId;

                                string message = EnumDescription.GetEnumDescription((ContractDeploymentResultEnum)result.ResultId);
                                Console.WriteLine($">> Message information: {message}");
                            }
                            else
                            {
                                web3 = new Web3(account, new RpcClient(new Uri(blockchainInfo.RPCUrl), null, null, new HttpClientHandler() { MaxConnectionsPerServer = 10, UseProxy = false }));

                                Console.WriteLine($">> Exception: {ex.Message}, StackTrace: {ex.StackTrace}");

                                if (ex.InnerException != null)
                                {
                                    Console.WriteLine($">> Inner Exception Message: {ex.InnerException.Message}, Inner Exception StackTrace: {ex.InnerException.StackTrace}");
                                }
                            }
                        }
                        finally
                        {
                            receipt = null;
                            contract = null;
                            contractDeploymentDataHelper.Dispose();
                        }
                    };

                    String consumerTag = channel.BasicConsume(Settings.ContractDeploymentQueueName, false, consumer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($">> Exception: {ex.Message}, StackTrace: {ex.StackTrace}");

                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($">> Inner Exception Message: {ex.InnerException.Message}, Inner Exception StackTrace: {ex.InnerException.StackTrace}");
                    }
                }
            });

            // handle Control+C or Control+Break
            Console.CancelKeyPress += (o, e) =>
            {
                Console.WriteLine("Exit");

                // allow the manin thread to continue and exit...
                waitHandle.Set();
            };

            // wait
            waitHandle.WaitOne();
        }

        private static string StatusString(BigInteger status)
        {
            return (status == 0) ? "fail" : "success";
        }
    }
}