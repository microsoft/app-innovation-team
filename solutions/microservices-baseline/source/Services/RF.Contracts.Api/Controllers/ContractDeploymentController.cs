using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RF.Contracts.Api.Helpers.Queue;
using RF.Contracts.Domain.Entities.Contracts_Api;
using RF.Contracts.Domain.Entities.KeyVault;
using RF.Contracts.Domain.Entities.Queue;
using RF.Contracts.Domain.Enums;
using RF.Contracts.Domain.Enums.Contracts_Api;
using RF.Contracts.Domain.Exceptions;
using System;
using System.Threading.Tasks;

namespace RF.Contracts.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractDeploymentController : ControllerBase
    {
        private readonly ILogger logger = null;

        public ContractDeploymentController(ILogger<ContractDeploymentController> logger)
        {
            this.logger = logger;
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> Post([FromBody]ContractDeploymentRequest model)
        {
            // non-forced-to-disposal
            ContractDeploymentResult result = new ContractDeploymentResult
            {
                IsSucceded = true,
                ResultId = (int)ContractDeploymentResultEnum.Success
            };

            // forced-to-disposal
            KeyVaultConnectionInfo keyVaultConnectionInfo = null;

            try
            {
                if (string.IsNullOrEmpty(model.Name))
                    throw new BusinessException((int)ContractDeploymentResultEnum.FailedEmptyName);

                if (string.IsNullOrEmpty(model.Description))
                    throw new BusinessException((int)ContractDeploymentResultEnum.FailedEmptyDescription);

                keyVaultConnectionInfo = new KeyVaultConnectionInfo()
                {
                    CertificateName = Settings.KeyVaultCertificateName,
                    ClientId = Settings.KeyVaultClientId,
                    ClientSecret = Settings.KeyVaultClientSecret,
                    KeyVaultIdentifier = Settings.KeyVaultIdentifier
                };

                using (MessageQueueHelper messageQueueHelper = new MessageQueueHelper())
                {
                    ContractDeploymentMessage contractDeploymentMessage = new ContractDeploymentMessage()
                    {
                        name = model.Name,
                        description = model.Description
                    };

                    await messageQueueHelper.QueueMessageAsync(contractDeploymentMessage, Settings.ContractDeploymentQueueName, keyVaultConnectionInfo);
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
                    result.ResultId = (int)ContractDeploymentResultEnum.Failed;

                    this.logger.LogError($">> Exception: {ex.Message}, StackTrace: {ex.StackTrace}");

                    if (ex.InnerException != null)
                    {
                        this.logger.LogError($">> Inner Exception Message: {ex.InnerException.Message}, Inner Exception StackTrace: {ex.InnerException.StackTrace}");
                    }
                }
            }
            finally
            {
                keyVaultConnectionInfo = null;

                GC.Collect();
            }

            string message = EnumDescription.GetEnumDescription((ContractDeploymentResultEnum)result.ResultId);
            this.logger.LogInformation($">> Message information: {message}");

            return (result.IsSucceded) ? (ActionResult)new OkObjectResult(new { message = message }) : (ActionResult)new BadRequestObjectResult(new { message = message });
        }
    }
}