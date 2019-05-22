using System.ComponentModel;

namespace BotApp.Luis.Router.Domain.Enums
{
    public enum LuisDiscoveryResponseEnum
    {
        [Description("Luis discovered successfully.")]
        Success,

        [Description("Text can not be empty.")]
        FailedEmptyText,

        [Description("There was a problem in the LUIS discovery process.")]
        Failed
    }
}