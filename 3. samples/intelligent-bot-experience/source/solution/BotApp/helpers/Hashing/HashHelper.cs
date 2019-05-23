using System.Text;

namespace BotApp
{
    public class HashHelper
    {
        public static string GetSha256Hash(string inputvalue)
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

        public static string GetSha256Hash(byte[] inputvalue)
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