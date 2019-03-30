using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RF.Identity.Api.Helpers.Data;
using RF.Identity.Api.Helpers.Hashing;
using RF.Identity.Domain.Entities.Data;
using RF.Identity.Domain.Entities.Identity_Api;
using RF.Identity.Domain.Enums;
using RF.Identity.Domain.Enums.Identity_Api;
using RF.Identity.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RF.Identity.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserAuthenticationController : ControllerBase
    {
        private readonly ILogger logger = null;

        public UserAuthenticationController(ILogger<UserAuthenticationController> logger)
        {
            this.logger = logger;
        }

        [HttpPost]
        public IActionResult Post([FromBody]UserAuthenticationRequest model)
        {
            // non-forced-to-disposal
            string response = string.Empty;
            UserAuthenticationResult result = new UserAuthenticationResult
            {
                IsSucceded = true,
                ResultId = (int)UserAuthenticationResultEnum.Success
            };

            // forced-to-disposal
            MongoDBConnectionInfo mongoDBConnectionInfo = null;
            SymmetricSecurityKey secretKey = null;
            SigningCredentials signinCredentials = null;
            User user = null;

            try
            {
                if (string.IsNullOrEmpty(model.Email))
                    throw new BusinessException((int)UserAuthenticationResultEnum.FailedEmptyEmail);

                if (string.IsNullOrEmpty(model.Password))
                    throw new BusinessException((int)UserAuthenticationResultEnum.FailedEmptyPassword);

                mongoDBConnectionInfo = new MongoDBConnectionInfo()
                {
                    ConnectionString = Settings.ConnectionString,
                    DatabaseId = Settings.DatabaseId,
                    UserCollection = Settings.UserCollection
                };

                using (UserAuthenticationDataHelper userAuthenticationDataHelper = new UserAuthenticationDataHelper(mongoDBConnectionInfo))
                {
                    user = userAuthenticationDataHelper.GetUser(model.Email);
                }

                if (user == null)
                    throw new BusinessException((int)UserAuthenticationResultEnum.FailedNotExistsInactiveAccount);

                string password = string.Empty;
                using (HashHelper hh = new HashHelper())
                {
                    password = hh.GetSha256Hash(model.Password);
                }

                if (user.email != model.Email || user.password != password)
                    throw new BusinessException((int)UserAuthenticationResultEnum.FailedIncorrectCredentials);

                secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Settings.AuthorizationKey));
                signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

                var tokeOptions = new JwtSecurityToken(
                    issuer: "https://rf.identity.api",
                    audience: "https://rf.identity.api",
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
                    result.ResultId = (int)UserAuthenticationResultEnum.Failed;

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
                secretKey = null;
                signinCredentials = null;
                user = null;

                GC.Collect();
            }

            string message = EnumDescription.GetEnumDescription((UserAuthenticationResultEnum)result.ResultId);
            this.logger.LogInformation($">> Message information: {message}");

            return (result.IsSucceded) ? (ActionResult)new OkObjectResult(new { token = response }) : (ActionResult)new BadRequestObjectResult(new { message = message });
        }
    }
}