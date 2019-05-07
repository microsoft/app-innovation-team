using System;
using System.Collections.Generic;
using System.Text;

namespace RF.ContentSearch.Domain.Entities.Queue
{
    public class ContentApprovalMessage
    {
        public string Bidder { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string ApprovedBy { get; set; }
        public Uri ContentUri { get; set; }
    }
}
