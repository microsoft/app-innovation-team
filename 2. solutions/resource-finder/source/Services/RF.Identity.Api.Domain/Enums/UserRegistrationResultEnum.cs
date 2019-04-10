using System.ComponentModel;

namespace RF.Identity.Api.Domain.Enums
{
    public enum UserRegistrationResultEnum
    {
        [Description("Registration request completed successfully, please wait the activation confirmation email.")]
        Success,

        [Description("The user already exists, registration not required.")]
        SuccessAlreadyExists,

        [Description("The fullname can not be empty.")]
        FailedEmptyFullname,

        [Description("There was a problem in the registration.")]
        Failed
    }
}