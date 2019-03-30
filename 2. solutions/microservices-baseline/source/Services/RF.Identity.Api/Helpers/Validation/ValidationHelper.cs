using RF.Identity.Api.Helpers.Base;
using System.Text.RegularExpressions;

namespace RF.Identity.Api.Helpers.Validation
{
    public class ValidationHelper : BaseHelper
    {
        private string USERNAME_PATTERN = @"^[A-Za-z\d]{6,20}$";
        private string EMAIL_PATTERN = @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z";

        public bool IsValidUsername(string username)
        {
            Regex regex = new Regex(USERNAME_PATTERN);
            Match match = regex.Match(username);
            return match.Success;
        }

        public bool IsValidEmail(string email)
        {
            return Regex.IsMatch(email, EMAIL_PATTERN, RegexOptions.IgnoreCase);
        }
    }
}