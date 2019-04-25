using System;
using System.Collections.Generic;
using System.Text;

namespace RF.ContentSearch.Api.Domain.Requests
{
    public class ContentSubmissionRequest
    {
        public string Bidder { get; set; }
        public string Approver { get; set; }
        public Uri ContentUri { get; set; }

    }
}
