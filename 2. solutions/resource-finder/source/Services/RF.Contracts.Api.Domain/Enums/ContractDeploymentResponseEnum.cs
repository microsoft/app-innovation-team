using System.ComponentModel;

namespace RF.Contracts.Api.Domain.Enums
{
    public enum ContractDeploymentResponseEnum
    {
        [Description("Contract deployment queued successfully.")]
        Success,

        [Description("The name can not be empty.")]
        FailedEmptyName,

        [Description("The description can not be empty.")]
        FailedEmptyDescription,

        [Description("There was a problem in the contract deployment.")]
        Failed
    }
}