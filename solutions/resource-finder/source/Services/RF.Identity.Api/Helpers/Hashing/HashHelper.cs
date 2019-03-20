using RF.Identity.Api.Helpers.Base;
using System.Text;

namespace RF.Identity.Api.Helpers.Hashing
{
    public class HashHelper : BaseHelper
    {
        public string GetSha256Hash(string inputvalue)
        {
            var crypt = System.Security.Cryptography.SHA256.Create();
            var hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(inputvalue));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        public string GetSha256Hash(byte[] inputvalue)
        {
            var crypt = System.Security.Cryptography.SHA256.Create();
            var hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(inputvalue);
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }
    }
}