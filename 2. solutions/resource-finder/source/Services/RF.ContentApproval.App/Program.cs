using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RF.ContentSearch.Api.Domain.Enums;
using RF.ContentSearch.Api.Domain.Responses;
using RF.ContentSearch.Api.Domain.Settings;
using RF.ContentSearch.Api.Helpers.KeyVault;
using RF.ContentSearch.Domain.Entities.Data;
using RF.ContentSearch.Domain.Entities.KeyVault;
using RF.ContentApproval.App.Mail;
using SendGrid.Helpers.Mail;
using RF.ContentSearch.Domain.Exceptions;
using RF.ContentSearch.Domain.Entities.Queue;
using RF.ContentSearch.Domain.Enums;

namespace RF.ContentApproval.App
{
    internal class Program
    {
        // AutoResetEvent to signal when to exit the application
        private static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);
        private static KeyVaultConnectionInfo keyVaultConnectionInfo = null;
        private static MongoDBConnectionInfo mongoDBConnectionInfo = null;
        private static string secret = string.Empty;

        static void Main(string[] args)
        {
            Task.Run(() =>
            {
                try
                {
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

                    channel.QueueDeclare(queue: ApplicationSettings.ContentApprovalQueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                    arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    var consumer = new AsyncEventingBasicConsumer(channel);

                    ContentApprovalResponse response = new ContentApprovalResponse
                    {         
                        IsSucceded = true,
                        ResultId = (int)ContentApprovalResponseEnum.Success
                    };

                    consumer.Received += async (model, ea) =>
                    {
                        string jsonDecrypted = string.Empty;
                        string jsonEncrypted = string.Empty;
                        RF.ContentApproval.App.Mail.MailHelper mailHelper = null;

                        try
                        {
                            byte[] body = ea.Body;
                            var message = Encoding.UTF8.GetString(body);

                            var decrypted = string.Empty;
                            decrypted = NETCore.Encrypt.EncryptProvider.AESDecrypt(message, secret);

                            var contentApprovalMessage = JsonConvert.DeserializeObject<ContentApprovalMessage>(decrypted);
                            var contentUri = contentApprovalMessage.ContentUri;
                            //todo:  copy the content into the index
                            //todo:  store approval
                        }
                        catch (Exception ex)
                        {
                            if (ex is BusinessException)
                            {
                                response.IsSucceded = false;
                                response.ResultId = ((BusinessException)ex).ResultId;

                                string message = EnumDescription.GetEnumDescription((ContentSubmissionResponseEnum)response.ResultId);
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

                        }
                    };
                }
                catch (Exception ex)
                {

                }
                finally
                {

                }
            });
        }

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
            ApplicationSettings.ContentApprovalQueueName = Configuration.GetSection("ApplicationSettings:ContentApprovalQueueName")?.Value;
            ApplicationSettings.RabbitMQUsername = Configuration.GetSection("ApplicationSettings:RabbitMQUsername")?.Value;
            ApplicationSettings.RabbitMQPassword = Configuration.GetSection("ApplicationSettings:RabbitMQPassword")?.Value;
            ApplicationSettings.RabbitMQHostname = Configuration.GetSection("ApplicationSettings:RabbitMQHostname")?.Value;
            ApplicationSettings.RabbitMQPort = Convert.ToInt16(Configuration.GetSection("ApplicationSettings:RabbitMQPort")?.Value);
            ApplicationSettings.KeyVaultCertificateName = Configuration.GetSection("ApplicationSettings:KeyVaultCertificateName")?.Value;
            ApplicationSettings.KeyVaultClientId = Configuration.GetSection("ApplicationSettings:KeyVaultClientId")?.Value;
            ApplicationSettings.KeyVaultClientSecret = Configuration.GetSection("ApplicationSettings:KeyVaultClientSecret")?.Value;
            ApplicationSettings.KeyVaultIdentifier = Configuration.GetSection("ApplicationSettings:KeyVaultIdentifier")?.Value;
            ApplicationSettings.KeyVaultEncryptionKey = Configuration.GetSection("ApplicationSettings:KeyVaultEncryptionKey")?.Value;

            keyVaultConnectionInfo = new KeyVaultConnectionInfo()
            {
                CertificateName = ApplicationSettings.KeyVaultCertificateName,
                ClientId = ApplicationSettings.KeyVaultClientId,
                ClientSecret = ApplicationSettings.KeyVaultClientSecret,
                KeyVaultIdentifier = ApplicationSettings.KeyVaultIdentifier
            };

            mongoDBConnectionInfo = new MongoDBConnectionInfo()
            {
                ConnectionString = ApplicationSettings.ConnectionString,
                DatabaseId = ApplicationSettings.DatabaseId,
                UserCollection = ApplicationSettings.UserCollection
            };

            using (KeyVaultHelper keyVaultHelper = new KeyVaultHelper(keyVaultConnectionInfo))
            {
                secret = keyVaultHelper.GetVaultKeyAsync(ApplicationSettings.KeyVaultEncryptionKey).Result;
            }
        }
    }
}
