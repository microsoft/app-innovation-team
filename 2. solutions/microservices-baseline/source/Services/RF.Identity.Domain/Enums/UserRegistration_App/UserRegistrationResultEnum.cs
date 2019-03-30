using System.ComponentModel;

namespace RF.Identity.Domain.Enums.UserRegistration_App
{
    public enum UserRegistrationResultEnum
    {
        [Description("User activation successfully.")]
        Success,

        [Description("The email already exists.")]
        FailedEmailAlreadyExists,

        [Description("There was a problem in the user activation.")]
        Failed
    }
}