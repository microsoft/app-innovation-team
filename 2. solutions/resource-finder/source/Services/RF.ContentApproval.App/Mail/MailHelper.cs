using RF.ContentApproval.App.Helpers;
using RF.ContentSearch.Api.Domain.Settings;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RF.ContentApproval.App.Mail
{
    public class MailHelper : BaseHelper
    {
        public async Task SendContentSubmissionEmailAsync(
            string emailTo, string emailToFullname, Uri contentUri)
        {
            //configure sendergrid api.
            string apiKey = ApplicationSettings.SendGridAPIKey;
            var client = new SendGrid.SendGridClient(apiKey);

            //build mail.
            var from = new EmailAddress("registration@company.com", "Resource Finder registration");
            var to = new EmailAddress($"{emailTo}", $"{emailToFullname}");
            string subject = "Resource Finder notification: Activation completed";

            var plainTextContent = string.Empty;

            var htmlContent =
                $"Hello {emailToFullname}!" +
                $"<br/><br/>" +
                $"Content has been submitted for approval." +
                $"<br/>" +
                $"You can find the content at { contentUri.AbsoluteUri }" +
                $"<br/><br/>" +
                $"Thanks for using Resource Finder," +
                $"<br/>" +
                $"Resource Finder team!";

            var msg = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
