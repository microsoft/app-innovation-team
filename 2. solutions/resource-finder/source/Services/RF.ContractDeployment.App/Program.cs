using Microsoft.Extensions.Configuration;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RF.ContractDeployment.App.Domain.Blockchain;
using RF.ContractDeployment.App.Domain.Enums;
using RF.ContractDeployment.App.Domain.Responses;
using RF.ContractDeployment.App.Domain.Settings;
using RF.ContractDeployment.App.Helpers.Data;
using RF.ContractDeployment.App.Helpers.KeyVault;
using RF.Contracts.Domain.Entities.Data;
using RF.Contracts.Domain.Entities.KeyVault;
using RF.Contracts.Domain.Entities.Queue;
using RF.Contracts.Domain.Enums;
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
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.WriteLine($"Environment: {environment}");

            var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.{environment}.json", true, true)
            .AddEnvironmentVariables();

            IConfigurationRoot Configuration = builder.Build();

            // Retrieve configuration from sections
            ApplicationSettings.ConnectionString = Configuration.GetSection("ApplicationSettings:ConnectionString")?.Value;
            ApplicationSettings.DatabaseId = Configuration.GetSection("ApplicationSettings:DatabaseId")?.Value;
            ApplicationSettings.ContractCollection = Configuration.GetSection("ApplicationSettings:ContractCollection")?.Value;
            ApplicationSettings.RabbitMQUsername = Configuration.GetSection("ApplicationSettings:RabbitMQUsername")?.Value;
            ApplicationSettings.RabbitMQPassword = Configuration.GetSection("ApplicationSettings:RabbitMQPassword")?.Value;
            ApplicationSettings.RabbitMQHostname = Configuration.GetSection("ApplicationSettings:RabbitMQHostname")?.Value;
            ApplicationSettings.RabbitMQPort = Convert.ToInt16(Configuration.GetSection("ApplicationSettings:RabbitMQPort")?.Value);
            ApplicationSettings.ContractDeploymentQueueName = Configuration.GetSection("ApplicationSettings:ContractDeploymentQueueName")?.Value;
            ApplicationSettings.KeyVaultCertificateName = Configuration.GetSection("ApplicationSettings:KeyVaultCertificateName")?.Value;
            ApplicationSettings.KeyVaultClientId = Configuration.GetSection("ApplicationSettings:KeyVaultClientId")?.Value;
            ApplicationSettings.KeyVaultClientSecret = Configuration.GetSection("ApplicationSettings:KeyVaultClientSecret")?.Value;
            ApplicationSettings.KeyVaultIdentifier = Configuration.GetSection("ApplicationSettings:KeyVaultIdentifier")?.Value;
            ApplicationSettings.KeyVaultEncryptionKey = Configuration.GetSection("ApplicationSettings:KeyVaultEncryptionKey")?.Value;
            ApplicationSettings.BlockchainRPCUrl = Configuration.GetSection("ApplicationSettings:BlockchainRPCUrl")?.Value;
            ApplicationSettings.BlockchainMasterAddress = Configuration.GetSection("ApplicationSettings:BlockchainMasterAddress")?.Value;
            ApplicationSettings.BlockchainMasterPrivateKey = Configuration.GetSection("ApplicationSettings:BlockchainMasterPrivateKey")?.Value;
            ApplicationSettings.BlockchainContractABI = Configuration.GetSection("ApplicationSettings:BlockchainContractABI")?.Value;
            ApplicationSettings.BlockchainContractByteCode = Configuration.GetSection("ApplicationSettings:BlockchainContractByteCode")?.Value;

            mongoDBConnectionInfo = new MongoDBConnectionInfo()
            {
                ConnectionString = ApplicationSettings.ConnectionString,
                DatabaseId = ApplicationSettings.DatabaseId,
                ContractCollection = ApplicationSettings.ContractCollection
            };

            keyVaultConnectionInfo = new KeyVaultConnectionInfo()
            {
                CertificateName = ApplicationSettings.KeyVaultCertificateName,
                ClientId = ApplicationSettings.KeyVaultClientId,
                ClientSecret = ApplicationSettings.KeyVaultClientSecret,
                KeyVaultIdentifier = ApplicationSettings.KeyVaultIdentifier
            };

            blockchainInfo = new BlockchainInfo()
            {
                RPCUrl = ApplicationSettings.BlockchainRPCUrl,
                MasterAddress = ApplicationSettings.BlockchainMasterAddress,
                MasterPrivateKey = ApplicationSettings.BlockchainMasterPrivateKey,
                ContractABI = ApplicationSettings.BlockchainContractABI,
                ContractByteCode = ApplicationSettings.BlockchainContractByteCode
            };

            using (KeyVaultHelper keyVaultHelper = new KeyVaultHelper(keyVaultConnectionInfo))
            {
                secret = keyVaultHelper.GetVaultKeyAsync(ApplicationSettings.KeyVaultEncryptionKey).Result;
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
                    factory.UserName = ApplicationSettings.RabbitMQUsername;
                    factory.Password = ApplicationSettings.RabbitMQPassword;
                    factory.HostName = ApplicationSettings.RabbitMQHostname;
                    factory.Port = ApplicationSettings.RabbitMQPort;
                    factory.RequestedHeartbeat = 60;
                    factory.DispatchConsumersAsync = true;

                    var connection = factory.CreateConnection();
                    var channel = connection.CreateModel();

                    channel.QueueDeclare(queue: ApplicationSettings.ContractDeploymentQueueName,
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.Received += async (model, ea) =>
                    {
                        ContractDeploymentResponse result = new ContractDeploymentResponse
                        {
                            IsSucceded = true,
                            ResultId = (int)ContractDeploymentResponseEnum.Success
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

                            //Console.WriteLine($">> Contract ABI: {blockchainInfo.ContractABI}");
                            //Console.WriteLine($">> Contract ByteCode: {blockchainInfo.ContractByteCode}");
                            //Console.WriteLine($">> Master Address: {blockchainInfo.MasterAddress}");

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

                                string message = EnumDescription.GetEnumDescription((ContractDeploymentResponseEnum)result.ResultId);
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

                    String consumerTag = channel.BasicConsume(ApplicationSettings.ContractDeploymentQueueName, false, consumer);
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