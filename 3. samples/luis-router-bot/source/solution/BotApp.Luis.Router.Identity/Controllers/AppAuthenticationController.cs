using BotApp.Luis.Router.Identity.Domain.Enums;
using BotApp.Luis.Router.Identity.Domain.Exceptions;
using BotApp.Luis.Router.Identity.Domain.KeyVault;
using BotApp.Luis.Router.Identity.Domain.Requests;
using BotApp.Luis.Router.Identity.Domain.Responses;
using BotApp.Luis.Router.Identity.Helpers.KeyVault;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BotApp.Luis.Router.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppAuthenticationController : ControllerBase
    {
        private readonly ILogger logger = null;

        public AppAuthenticationController(ILogger<AppAuthenticationController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]AppAuthenticationRequest model)
        {
            // non-forced-to-disposal
            string response = string.Empty;
            AppAuthenticationResponse result = new AppAuthenticationResponse
            {
                IsSucceded = true,
                ResultId = (int)AppAuthenticationResultEnum.Success
            };

            // forced-to-disposal
            KeyVaultConnectionInfo keyVaultConnectionInfo = null;
            SymmetricSecurityKey secretKey = null;
            SigningCredentials signinCredentials = null;

            try
            {
                if (string.IsNullOrEmpty(model.ApplicationCode))
                    throw new BusinessException((int)AppAuthenticationResultEnum.FailedEmptyClientApplicationCode);

                keyVaultConnectionInfo = new KeyVaultConnectionInfo()
                {
                    CertificateName = Settings.KeyVaultCertificateName,
                    ClientId = Settings.KeyVaultClientId,
                    ClientSecret = Settings.KeyVaultClientSecret,
                    KeyVaultIdentifier = Settings.KeyVaultIdentifier
                };

                string encryptionkey = string.Empty;
                string appcode = string.Empty;
                using (KeyVaultHelper keyVaultHelper = new KeyVaultHelper(keyVaultConnectionInfo))
                {
                    encryptionkey = await keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultEncryptionKey);
                    appcode = await keyVaultHelper.GetVaultKeyAsync(Settings.KeyVaultApplicationCode);
                }

                if (string.IsNullOrEmpty(appcode))
                    throw new BusinessException((int)AppAuthenticationResultEnum.FailedEmptyServerApplicationCode);

                var decrypted = string.Empty;
                decrypted = NETCore.Encrypt.EncryptProvider.AESDecrypt(model.ApplicationCode, encryptionkey);

                if (appcode != decrypted)
                    throw new BusinessException((int)AppAuthenticationResultEnum.FailedIncorrectCredentials);

                secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.AuthorizationKey));
                signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

                var tokeOptions = new JwtSecurityToken(
                    issuer: "https://BotApp.Luis.Router.Identity",
                    audience: "https://BotApp.Luis.Router.Identity",
                    claims: new List<Claim>(),
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: signinCredentials
                );

                response = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
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
                    result.ResultId = (int)AppAuthenticationResultEnum.Failed;

                    this.logger.LogError($">> Exception: {ex.Message}, StackTrace: {ex.StackTrace}");

                    if (ex.InnerException != null)
                    {
                        this.logger.LogError($">> Inner Exception Message: {ex.InnerException.Message}, Inner Exception StackTrace: {ex.InnerException.StackTrace}");
                    }
                }
            }
            finally
            {
                // clean forced-to-disposal
                keyVaultConnectionInfo = null;
                secretKey = null;
                signinCredentials = null;

                GC.Collect();
            }

            string message = EnumDescription.GetEnumDescription((AppAuthenticationResultEnum)result.ResultId);
            this.logger.LogInformation($">> Message information: {message}");

            return (result.IsSucceded) ? (ActionResult)new OkObjectResult(new { token = response }) : (ActionResult)new BadRequestObjectResult(new { message = message });
        }
    }
}