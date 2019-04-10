using System.ComponentModel;

namespace RF.UserRegistration.App.Domain.Enums
{
    public enum UserRegistrationResponseEnum
    {
        [Description("User activation successfully.")]
        Success,

        [Description("The email already exists.")]
        FailedEmailAlreadyExists,

        [Description("There was a problem in the user activation.")]
        Failed
    }
}