using System.ComponentModel;

namespace BotApp.Luis.Router.Identity.Domain.Enums
{
    public enum IdentityResultEnum
    {
        [Description("Application authentication successfully.")]
        Success,

        [Description("The application identity can not be empty.")]
        FailedEmptyAppIdentity,

        [Description("The application code is not valid.")]
        FailedIncorrectCredentials,

        [Description("There was a problem in the application authentication.")]
        Failed
    }
}