using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RF.Identity.Api.Helpers.Data;
using RF.Identity.Api.Helpers.Hashing;
using RF.Identity.Api.Helpers.Queue;
using RF.Identity.Api.Helpers.Validation;
using RF.Identity.Domain.Entities.Data;
using RF.Identity.Domain.Entities.Identity_Api;
using RF.Identity.Domain.Entities.KeyVault;
using RF.Identity.Domain.Entities.Queue;
using RF.Identity.Domain.Enums;
using RF.Identity.Domain.Enums.Identity_Api;
using RF.Identity.Domain.Exceptions;
using System;
using System.Threading.Tasks;

namespace RF.Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserRegistrationController : ControllerBase
    {
        private readonly ILogger logger = null;

        public UserRegistrationController(ILogger<UserRegistrationController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]UserRegistrationRequest model)
        {
            // non-forced-to-disposal
            UserRegistrationResult result = new UserRegistrationResult
            {
                IsSucceded = true,
                ResultId = (int)UserRegistrationResultEnum.Success
            };

            // forced-to-disposal
            MongoDBConnectionInfo mongoDBConnectionInfo = null;
            KeyVaultConnectionInfo keyVaultConnectionInfo = null;
            User user = null;

            try
            {
                if (string.IsNullOrEmpty(model.Fullname))
                    throw new BusinessException((int)UserRegistrationResultEnum.FailedEmptyFullname);

                if (string.IsNullOrEmpty(model.Email))
                    throw new BusinessException((int)UserRegistrationResultEnum.FailedEmptyEmail);

                if (string.IsNullOrEmpty(model.Password))
                    throw new BusinessException((int)UserRegistrationResultEnum.FailedEmptyPassword);

                using (ValidationHelper validationHelper = new ValidationHelper())
                {
                    bool validationEmail = validationHelper.IsValidEmail(model.Email);

                    if (!validationEmail)
                        throw new BusinessException((int)UserRegistrationResultEnum.FailedNotValidEmail);
                }

                mongoDBConnectionInfo = new MongoDBConnectionInfo()
                {
                    ConnectionString = Settings.ConnectionString,
                    DatabaseId = Settings.DatabaseId,
                    UserCollection = Settings.UserCollection
                };

                using (UserRegistrationDataHelper userRegistrationDataHelper = new UserRegistrationDataHelper(mongoDBConnectionInfo))
                {
                    user = userRegistrationDataHelper.GetUser(model.Email);
                }

                if (user != null)
                    throw new BusinessException((int)UserRegistrationResultEnum.FailedEmailAlreadyExists);

                string password = string.Empty;
                using (HashHelper hashHelper = new HashHelper())
                {
                    password = hashHelper.GetSha256Hash(model.Password);
                }

                keyVaultConnectionInfo = new KeyVaultConnectionInfo()
                {
                    CertificateName = Settings.KeyVaultCertificateName,
                    ClientId = Settings.KeyVaultClientId,
                    ClientSecret = Settings.KeyVaultClientSecret,
                    KeyVaultIdentifier = Settings.KeyVaultIdentifier
                };

                using (MessageQueueHelper messageQueueHelper = new MessageQueueHelper())
                {
                    UserRegistrationMessage userRegistrationMessage = new UserRegistrationMessage()
                    {
                        fullname = model.Fullname,
                        email = model.Email,
                        password = password
                    };

                    await messageQueueHelper.QueueMessageAsync(userRegistrationMessage, Settings.UserRegistrationQueueName, keyVaultConnectionInfo);
                }
            }
            catch (Exception ex)
            {
                result.IsSucceded = false;

                if (ex is BusinessException)
                {
                    result.ResultId = ((BusinessException)ex).ResultId;
                }
                else
                {
                    result.ResultId = (int)UserRegistrationResultEnum.Failed;

                    this.logger.LogError($">> Exception: {ex.Message}, StackTrace: {ex.StackTrace}");

                    if (ex.InnerException != null)
                    {
                        this.logger.LogError($">> Inner Exception Message: {ex.InnerException.Message}, Inner Exception StackTrace: {ex.InnerException.StackTrace}");
                    }
                }
            }
            finally
            {
                mongoDBConnectionInfo = null;
                keyVaultConnectionInfo = null;
                user = null;

                GC.Collect();
            }

            string message = EnumDescription.GetEnumDescription((UserRegistrationResultEnum)result.ResultId);
            this.logger.LogInformation($">> Message information: {message}");

            return (result.IsSucceded) ? (ActionResult)new OkObjectResult(new { message = message }) : (ActionResult)new BadRequestObjectResult(new { message = message });
        }
    }
}