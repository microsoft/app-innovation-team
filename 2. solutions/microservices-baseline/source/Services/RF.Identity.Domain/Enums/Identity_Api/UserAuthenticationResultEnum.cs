using System.ComponentModel;

namespace RF.Identity.Domain.Enums.Identity_Api
{
    public enum UserAuthenticationResultEnum
    {
        [Description("Authentication successfully.")]
        Success,

        [Description("The email can not be empty.")]
        FailedEmptyEmail,

        [Description("The password can not be empty.")]
        FailedEmptyPassword,

        [Description("The email or password is not valid.")]
        FailedIncorrectCredentials,

        [Description("The user is not registered or active.")]
        FailedNotExistsInactiveAccount,

        [Description("There was a problem in the authentication.")]
        Failed
    }
}