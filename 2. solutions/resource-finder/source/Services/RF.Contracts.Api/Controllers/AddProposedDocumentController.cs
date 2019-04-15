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
using QuorumDemo.Core;
using QuorumDemo.Core.Models;
using System.Collections.Generic;

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
        public async Task<IActionResult> Post([FromBody]ContractDeploymentRequest PostRequest)
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
                if (string.IsNullOrEmpty(PostRequest.Name))
                    throw new BusinessException((int)ContractDeploymentResponseEnum.FailedEmptyName);

                if (string.IsNullOrEmpty(PostRequest.Description))
                    throw new BusinessException((int)ContractDeploymentResponseEnum.FailedEmptyDescription);

                keyVaultConnectionInfo = new KeyVaultConnectionInfo()
                {
                    CertificateName = ApplicationSettings.KeyVaultCertificateName,
                    ClientId = ApplicationSettings.KeyVaultClientId,
                    ClientSecret = ApplicationSettings.KeyVaultClientSecret,
                    KeyVaultIdentifier = ApplicationSettings.KeyVaultIdentifier
                };

                
                //--- Make a call into the Quorum Helper and call the contract --/

                var abiFile = System.IO.File.OpenText("/../../../Contracts/ProposalFile.abi");
                var abi = abiFile.ReadToEnd();

                var byteCodeFile = System.IO.File.OpenText("/../../../Contracts/ProposalFile.bin");
                var byteCode = "0x" + byteCodeFile.ReadToEnd();


                var ContractInfo = new ContractInfo
                {
                    ContractABI = abi,
                    ContractByteCode = byteCode
                };

                var RPC_URL = ""; // based n who is logged in we know the RPC URL
                var CONTRACT_ADDRESS = ""; // THIS WILL REMAIN CONSTANT ONCE WE HAVE DEPLOYED THE CONTRACTS

                // following is for getting private key but if we have it its OK. 
                var ACCOUNT_JSON_FILE = "";
                var PASSWORD = "";

                var PRIVATE_FOR_KEYS = new List<string>() { "aasdfsadfsadf" };


                var account = QuorumDemo.Core.AccountHelper.DecryptAccount(ACCOUNT_JSON_FILE, PASSWORD);

                // -- SET WEB3 Handler -- //
                var quorumHelper = new QuorumContractHelper(RPC_URL);

                var txResult =  await quorumHelper.CreateTransactionAsync(
                    CONTRACT_ADDRESS,
                    ContractInfo,
                    "Register",
                    account,
                    new object[] { PostRequest.Name, PostRequest.Description, "the hash from IPFS"},
                    PRIVATE_FOR_KEYS
                    );

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