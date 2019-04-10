using System.ComponentModel;

namespace RF.ContractDeployment.App.Domain.Enums
{
    public enum ContractDeploymentResponseEnum
    {
        [Description("User activation successfully.")]
        Success,

        [Description("The user not exists in the database.")]
        FailedUserNotExists,

        [Description("There was a problem in the user activation.")]
        Failed
    }
}