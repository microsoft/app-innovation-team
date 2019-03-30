using System.ComponentModel;

namespace RF.Contracts.Domain.Enums.ContractDeployment_App
{
    public enum ContractDeploymentResultEnum
    {
        [Description("User activation successfully.")]
        Success,

        [Description("The user not exists in the database.")]
        FailedUserNotExists,

        [Description("There was a problem in the user activation.")]
        Failed
    }
}