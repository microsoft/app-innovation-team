using System.ComponentModel;

namespace BotApp.Luis.Router.Identity.Domain.Enums
{
    public enum AppAuthenticationResultEnum
    {
        [Description("Application authentication successfully.")]
        Success,

        [Description("The application code can not be empty.")]
        FailedEmptyClientApplicationCode,

        [Description("The application code can not be empty on the server.")]
        FailedEmptyServerApplicationCode,

        [Description("The application code is not valid.")]
        FailedIncorrectCredentials,

        [Description("There was a problem in the application authentication.")]
        Failed
    }
}