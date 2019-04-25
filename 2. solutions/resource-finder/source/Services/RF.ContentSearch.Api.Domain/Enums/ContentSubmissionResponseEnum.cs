using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace RF.ContentSearch.Api.Domain.Enums
{
    public enum ContentSubmissionResponseEnum
    {
        [Description("Content approved successfully.")]
        Success,

        [Description("The name can not be empty.")]
        FailedEmptyName,

        [Description("There was a problem in the contract deployment.")]
        Failed,

        [Description("A user with this email address does not exist in the system.")]
        UserDoesNotExist
    }
}
