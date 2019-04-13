using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RF.Contracts.Api.Domain.Enums;
using RF.Contracts.Api.Domain.Requests;
using RF.Contracts.Api.Domain.Responses;
using RF.Contracts.Api.Domain.Settings;
using RF.Contracts.Api.Helpers.Queue;
using RF.Contracts.Domain.Entities.KeyVault;
using RF.Contracts.Domain.Entities.Queue;
using RF.Contracts.Domain.Enums;
using RF.Contracts.Domain.Exceptions;
using System;
using System.Threading.Tasks;

namespace RF.Contracts.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddProposedDocumentController : ControllerBase
    {
        private readonly ILogger logger = null;

        public AddProposedDocumentController(ILogger<AddProposedDocumentController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [Authorize(Policy = "RF-Users")]
        public async Task<IActionResult> Post([FromBody]ContractDeploymentRequest model)
        {
            // non-forced-to-disposal
            ContractDeploymentResponse result = new ContractDeploymentResponse
            {
                IsSucceded = true,
                ResultId = (int)ContractDeploymentResponseEnum.Success
            };

            // forced-to-disposal
            KeyVaultConnectionInfo keyVaultConnectionInfo = null;

            try
            {
                if (string.IsNullOrEmpty(model.Name))
                    throw new BusinessException((int)ContractDeploymentResponseEnum.FailedEmptyName);

                if (string.IsNullOrEmpty(model.Description))
                    throw new BusinessException((int)ContractDeploymentResponseEnum.FailedEmptyDescription);

                keyVaultConnectionInfo = new KeyVaultConnectionInfo()
                {
                    CertificateName = ApplicationSettings.KeyVaultCertificateName,
                    ClientId = ApplicationSettings.KeyVaultClientId,
                    ClientSecret = ApplicationSettings.KeyVaultClientSecret,
                    KeyVaultIdentifier = ApplicationSettings.KeyVaultIdentifier
                };

                using (MessageQueueHelper messageQueueHelper = new MessageQueueHelper())
                {
                    ContractDeploymentMessage contractDeploymentMessage = new ContractDeploymentMessage()
                    {
                        name = model.Name,
                        description = model.Description
                    };

                    await messageQueueHelper.QueueMessageAsync(contractDeploymentMessage, ApplicationSettings.ContractDeploymentQueueName, keyVaultConnectionInfo);
                }

                //TODO: Make a call into the Quorum Helper and create the contract


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
                    result.ResultId = (int)ContractDeploymentResponseEnum.Failed;

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

            string message = EnumDescription.GetEnumDescription((ContractDeploymentResponseEnum)result.ResultId);
            this.logger.LogInformation($">> Message information: {message}");

            return (result.IsSucceded) ? (ActionResult)new OkObjectResult(new { message = message }) : (ActionResult)new BadRequestObjectResult(new { message = message });
        }
    }
}