using System.ComponentModel;

namespace RF.Identity.Domain.Enums.Identity_Api
{
    public enum UserRegistrationResultEnum
    {
        [Description("Registration request completed successfully, please wait the activation confirmation email.")]
        Success,

        [Description("The fullname can not be empty.")]
        FailedEmptyFullname,

        [Description("The email can not be empty.")]
        FailedEmptyEmail,

        [Description("The password can not be empty.")]
        FailedEmptyPassword,

        [Description("The email is not valid.")]
        FailedNotValidEmail,

        [Description("The email already exists.")]
        FailedEmailAlreadyExists,

        [Description("There was a problem in the registration.")]
        Failed
    }
}