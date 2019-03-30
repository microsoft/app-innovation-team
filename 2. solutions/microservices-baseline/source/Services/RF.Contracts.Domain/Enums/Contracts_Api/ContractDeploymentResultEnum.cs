using System.ComponentModel;

namespace RF.Contracts.Domain.Enums.Contracts_Api
{
    public enum ContractDeploymentResultEnum
    {
        [Description("Contract deployment successfully.")]
        Success,

        [Description("The name can not be empty.")]
        FailedEmptyName,

        [Description("The description can not be empty.")]
        FailedEmptyDescription,

        [Description("There was a problem in the contract deployment.")]
        Failed
    }
}