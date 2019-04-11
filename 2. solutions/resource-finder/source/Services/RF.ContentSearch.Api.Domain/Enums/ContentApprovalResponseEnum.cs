using System.ComponentModel;

namespace RF.ContentSearch.Api.Domain.Enums
{
    public enum ContentApprovalResponseEnum
    {
        [Description("Content approved successfully.")]
        Success,

        [Description("The name can not be empty.")]
        FailedEmptyName,

        [Description("There was a problem in the contract deployment.")]
        Failed
    }
}