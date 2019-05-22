using BotApp.Luis.Router.Identity.Domain.Enums;
using BotApp.Luis.Router.Identity.Domain.Exceptions;
using BotApp.Luis.Router.Identity.Domain.Requests;
using BotApp.Luis.Router.Identity.Domain.Responses;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BotApp.Luis.Router.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private readonly TelemetryClient telemetry = null;
        private readonly ILogger logger = null;

        public IdentityController(TelemetryClient telemetry, ILogger<IdentityController> logger)
        {
            this.telemetry = telemetry;
            this.logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody]IdentityRequest model)
        {
            this.telemetry.TrackEvent("Identity Post");

            // non-forced-to-disposal
            string response = string.Empty;
            IdentityResponse result = new IdentityResponse
            {
                IsSucceded = true,
                ResultId = (int)IdentityResultEnum.Success
            };

            // forced-to-disposal
            SymmetricSecurityKey secretKey = null;
            SigningCredentials signinCredentials = null;

            try
            {
                if (string.IsNullOrEmpty(model.AppIdentity))
                    throw new BusinessException((int)IdentityResultEnum.FailedEmptyAppIdentity);

                var decrypted = string.Empty;
                decrypted = NETCore.Encrypt.EncryptProvider.AESDecrypt(model.AppIdentity, Startup.EncryptionKey);

                dynamic data = JsonConvert.DeserializeObject(decrypted);
                string decryptedAppCode = data?.appcode;

                if (Startup.ApplicationCode != decryptedAppCode)
                    throw new BusinessException((int)IdentityResultEnum.FailedIncorrectCredentials);

                DateTime when = data?.timestamp;
                if (when < DateTime.UtcNow.AddMinutes(-5))
                    return Unauthorized();

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
                    result.ResultId = (int)IdentityResultEnum.Failed;

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
                secretKey = null;
                signinCredentials = null;

                GC.Collect();
            }

            string message = EnumDescription.GetEnumDescription((IdentityResultEnum)result.ResultId);
            this.logger.LogInformation($">> Message information: {message}");

            return (result.IsSucceded) ? (ActionResult)new OkObjectResult(new { token = response }) : (ActionResult)new BadRequestObjectResult(new { message = message });
        }
    }
}