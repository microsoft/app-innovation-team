using RF.UserRegistration.App.Helpers.Base;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace RF.UserRegistration.App.Helpers.Mail
{
    public class MailHelper : BaseHelper
    {
        public async Task SendRegistrationEmailAsync(
            string emailTo, string emailToFullname)
        {
            //configure sendergrid api.
            string apiKey = Settings.SendGridAPIKey;
            var client = new SendGrid.SendGridClient(apiKey);

            //build mail.
            var from = new EmailAddress("registration@company.com", "Resource Finder registration");
            var to = new EmailAddress($"{emailTo}", $"{emailToFullname}");
            string subject = "Resource Finder notification: Activation completed";

            var plainTextContent = string.Empty;

            var htmlContent =
                $"Hello {emailToFullname}!" +
                $"<br/><br/>" +
                $"We have successfully activate your account, you are now able to sign-in in the application." +
                $"<br/><br/>" +
                $"Thanks for using Resource Finder," +
                $"<br/>" +
                $"Resource Finder team!";

            var msg = SendGrid.Helpers.Mail.MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}