using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RF.ContentSearch.Api.Domain.Enums;
using RF.ContentSearch.Api.Domain.Requests;
using RF.ContentSearch.Api.Domain.Responses;
using RF.ContentSearch.Api.Domain.Settings;
using RF.ContentSearch.Api.Helpers.Queue;
using RF.ContentSearch.Domain.Entities.KeyVault;
using RF.ContentSearch.Domain.Entities.Queue;
using RF.ContentSearch.Domain.Enums;
using RF.ContentSearch.Domain.Exceptions;
using System;
using System.Threading.Tasks;

namespace RF.ContentSearch.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentApprovalController : ControllerBase
    {
        private readonly ILogger logger = null;

        public ContentApprovalController(ILogger<ContentApprovalController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        [Authorize(Policy = "RF-Users")]
        public async Task<IActionResult> Post([FromBody]ContentApprovalRequest model)
        {
            // non-forced-to-disposal
            ContractDeploymentResponse result = new ContractDeploymentResponse
            {
                IsSucceded = true,
                ResultId = (int)ContentApprovalResponseEnum.Success
            };

            // forced-to-disposal
            KeyVaultConnectionInfo keyVaultConnectionInfo = null;

            try
            {
                if (string.IsNullOrEmpty(model.Name))
                    throw new BusinessException((int)ContentApprovalResponseEnum.FailedEmptyName);

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
                    };
                    

                    await messageQueueHelper.QueueMessageAsync(contractDeploymentMessage, ApplicationSettings.ContractDeploymentQueueName, keyVaultConnectionInfo);
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
                    result.ResultId = (int)ContentApprovalResponseEnum.Failed;

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

            string message = EnumDescription.GetEnumDescription((ContentApprovalResponseEnum)result.ResultId);
            this.logger.LogInformation($">> Message information: {message}");

            return (result.IsSucceded) ? (ActionResult)new OkObjectResult(new { message = message }) : (ActionResult)new BadRequestObjectResult(new { message = message });
        }
    }
}