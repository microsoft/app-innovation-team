using Jdenticon;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RF.Identity.Domain.Entities.Blockchain;
using RF.Identity.Domain.Entities.Data;
using RF.Identity.Domain.Entities.KeyVault;
using RF.Identity.Domain.Entities.Queue;
using RF.Identity.Domain.Entities.UserRegistration_App;
using RF.Identity.Domain.Enums;
using RF.Identity.Domain.Enums.UserRegistration_App;
using RF.Identity.Domain.Exceptions;
using RF.UserRegistration.App.Helpers.Blockchain;
using RF.UserRegistration.App.Helpers.Data;
using RF.UserRegistration.App.Helpers.KeyVault;
using RF.UserRegistration.App.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RF.UserRegistration.App
{
    internal class Program
    {
        // AutoResetEvent to signal when to exit the application
        private static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);

        private static MongoDBConnectionInfo mongoDBConnectionInfo = null;
        private static KeyVaultConnectionInfo keyVaultConnectionInfo = null;
        private static string secret = string.Empty;
        private static List<string> wordlist = null;

        private static void Init()
        {
            var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true, true)
            .AddEnvironmentVariables();

            IConfigurationRoot Configuration = builder.Build();

            // Retrieve configuration from sections
            Settings.ConnectionString = Configuration.GetSection("ConnectionString")?.Value;
            Settings.DatabaseId = Configuration.GetSection("DatabaseId")?.Value;
            Settings.UserCollection = Configuration.GetSection("UserCollection")?.Value;
            Settings.RabbitMQUsername = Configuration.GetSection("RabbitMQUsername")?.Value;
            Settings.RabbitMQPassword = Configuration.GetSection("RabbitMQPassword")?.Value;
            Settings.RabbitMQHostname = Configuration.GetSection("RabbitMQHostname")?.Value;
            Settings.RabbitMQPort = Convert.ToInt16(Configuration.GetSection("RabbitMQPort")?.Value);
            Settings.UserRegistrationQueueName = Configuration.GetSection("UserRegistrationQueueName")?.Value;
            Settings.KeyVaultCertificateName = Configuration.GetSection("KeyVaultCertificateName")?.Value;
            Settings.KeyVaultClientId = Configuration.GetSection("KeyVaultClientId")?.Value;
            Settings.KeyVaultClientSecret = Configuration.GetSection("KeyVaultClientSecret")?.Value;
            Settings.KeyVaultIdentifier = Configuration.GetSection("KeyVaultIdentifier")?.Value;
            Settings.KeyVaultEncryptionKey = Configuration.GetSection("KeyVaultEncryptionKey")?.Value;
            Settings.SendGridAPIKey = Configuration.GetSection("SendGridAPIKey")?.Value;

            mongoDBConnectionInfo = new MongoDBConnectionInfo()
            {
                ConnectionString = Settings.ConnectionString,
                DatabaseId = Settings.DatabaseId,
                UserCollection = Settings.UserCollection
            };

            keyVaultConnectionInfo = new KeyVaultConnectionInfo()
            {
                CertificateName = Settings.KeyVaultCertificateName,
                ClientId = Settings.KeyVaultClientId,
                ClientSecret = Settings.KeyVaultClientSecret,
                KeyVaultIdentifier = Settings.KeyVaultIdentifier
            };

            using (KeyVaultHelper keyVaultHelper = new KeyVaultHelper(keyVaultConnectionInfo))
            {
                secret = keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultEncryptionKey).Result;
            }

            using (BlockchainHelper blockchainHelper = new BlockchainHelper())
            {
                wordlist = blockchainHelper.ReadMnemonic();
            }
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

                    channel.QueueDeclare(queue: Settings.UserRegistrationQueueName,
                                    durable: true,
                                    exclusive: false,
                                    autoDelete: false,
                                    arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.Received += async (model, ea) =>
                    {
                        UserRegistrationResult result = new UserRegistrationResult
                        {
                            IsSucceded = true,
                            ResultId = (int)UserRegistrationResultEnum.Success
                        };

                        // forced-to-disposal
                        WalletRegistrationInfo walletInfo = null;
                        NBitcoin.Wordlist nwordlist = null;
                        Nethereum.HdWallet.Wallet wallet = null;
                        Nethereum.Web3.Accounts.Account account = null;
                        string jsonDecrypted = string.Empty;
                        string jsonEncrypted = string.Empty;
                        User user = null;
                        UserActivationDataHelper userActivationDataHelper = null;
                        MailHelper mailHelper = null;

                        try
                        {
                            byte[] body = ea.Body;
                            var message = Encoding.UTF8.GetString(body);

                            var decrypted = string.Empty;
                            decrypted = NETCore.Encrypt.EncryptProvider.AESDecrypt(message, secret);

                            var obj_decrypted = JsonConvert.DeserializeObject<UserRegistrationMessage>(decrypted);

                            var aeskey_wallet = NETCore.Encrypt.EncryptProvider.CreateAesKey();
                            var key_wallet = aeskey_wallet.Key;

                            nwordlist = new NBitcoin.Wordlist(wordlist.ToArray(), ' ', "english");
                            wallet = new Nethereum.HdWallet.Wallet(nwordlist, NBitcoin.WordCount.Eighteen, key_wallet);
                            account = wallet.GetAccount(0);

                            walletInfo = new WalletRegistrationInfo();

                            walletInfo.address = account.Address;
                            walletInfo.privateKey = account.PrivateKey;
                            walletInfo.password = key_wallet;
                            walletInfo.mnemonic = string.Join(" ", wallet.Words);

                            jsonDecrypted = JsonConvert.SerializeObject(walletInfo);

                            var aeskey_data = NETCore.Encrypt.EncryptProvider.CreateAesKey();
                            var key_data = aeskey_data.Key;
                            jsonEncrypted = NETCore.Encrypt.EncryptProvider.AESEncrypt(jsonDecrypted, key_data);

                            string identicon = string.Empty;
                            try
                            {
                                Identicon.FromValue($"{account.Address}", size: 160).SaveAsPng($"{account.Address}.png");
                                byte[] binary = System.IO.File.ReadAllBytes($"{account.Address}.png");
                                identicon = Convert.ToBase64String(binary);
                                System.IO.File.Delete($"{account.Address}.png");
                                Console.WriteLine($">> Identicon deleted from local storage");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($">> Exception: {ex.Message}, StackTrace: {ex.StackTrace}");

                                if (ex.InnerException != null)
                                {
                                    Console.WriteLine($">> Inner Exception Message: {ex.InnerException.Message}, Inner Exception StackTrace: {ex.InnerException.StackTrace}");
                                }

                                try
                                {
                                    System.IO.File.Delete($"{account.Address}.png");
                                    Console.WriteLine($">> Identicon deleted from local storage");
                                }
                                catch { }
                            }

                            userActivationDataHelper = new UserActivationDataHelper(mongoDBConnectionInfo);

                            // get user by email
                            user = userActivationDataHelper.GetUser(obj_decrypted.email);

                            if (user != null)
                                throw new BusinessException((int)UserRegistrationResultEnum.FailedEmailAlreadyExists);

                            user = new User();
                            user.fullname = obj_decrypted.fullname;
                            user.email = obj_decrypted.email;
                            user.password = obj_decrypted.password;
                            user.role = "user";
                            user.address = account.Address;
                            user.dataenc = jsonEncrypted;
                            user.datakey = key_data;
                            user.identicon = identicon;

                            // register user
                            await userActivationDataHelper.RegisterUserAsync(user);

                            mailHelper = new MailHelper();

                            // send email
                            await mailHelper.SendRegistrationEmailAsync(user.email, user.fullname);
                            Console.WriteLine($">> Email: {user.email} activated successfully");

                            channel.BasicAck(ea.DeliveryTag, false);
                            Console.WriteLine($">> Acknowledgement completed, delivery tag: {ea.DeliveryTag}");
                        }
                        catch (Exception ex)
                        {
                            if (ex is BusinessException)
                            {
                                result.IsSucceded = false;
                                result.ResultId = ((BusinessException)ex).ResultId;

                                string message = EnumDescription.GetEnumDescription((UserRegistrationResultEnum)result.ResultId);
                                Console.WriteLine($">> Message information: {message}");
                            }
                            else
                            {
                                Console.WriteLine($">> Exception: {ex.Message}, StackTrace: {ex.StackTrace}");

                                if (ex.InnerException != null)
                                {
                                    Console.WriteLine($">> Inner Exception Message: {ex.InnerException.Message}, Inner Exception StackTrace: {ex.InnerException.StackTrace}");
                                }
                            }
                        }
                        finally
                        {
                            nwordlist = null;
                            wallet = null;
                            account = null;
                            walletInfo = null;
                            jsonDecrypted = null;
                            jsonEncrypted = null;
                            user = null;
                            userActivationDataHelper.Dispose();
                            mailHelper.Dispose();
                        }
                    };

                    String consumerTag = channel.BasicConsume(Settings.UserRegistrationQueueName, false, consumer);
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
    }
}