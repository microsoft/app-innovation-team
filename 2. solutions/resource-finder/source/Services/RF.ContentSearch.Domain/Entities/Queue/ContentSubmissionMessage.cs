using System;
using System.Collections.Generic;
using System.Text;

namespace RF.ContentSearch.Domain.Entities.Queue
{
    public class ContentSubmissionMessage
    {
        public string Bidder { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Approver { get; set; }
        public Uri ConentUri { get; set; }
    }
}
