using System.Security.Cryptography;
using System.Text;

namespace Penta_Server.StaticUtilits
{
    public static class PasswordManager
    {
        private static string shadowPass = "shadowPass_25";
        private static int _iterations = 20;

        public static bool IsPasswordStrong(string password)
        {
            return !string.IsNullOrEmpty(password) &&
               password.Length > 10 &&
               password.Any(char.IsDigit);
        }

        public static string Encrypt(this string inputPassword,  string userName)
        {
            using (Aes aes = Aes.Create())
            {
                byte[] saltBytes = Encoding.UTF8.GetBytes(userName);
                var key = new Rfc2898DeriveBytes(shadowPass, saltBytes, _iterations, HashAlgorithmName.SHA256);
                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs))
                        sw.Write(inputPassword);

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(this string cipherText, string userName)
        {
            using (Aes aes = Aes.Create())
            {
                byte[] saltBytes = Encoding.UTF8.GetBytes(userName);
                var key = new Rfc2898DeriveBytes(shadowPass, saltBytes, _iterations, HashAlgorithmName.SHA256);
                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                byte[] buffer = Convert.FromBase64String(cipherText);

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var ms = new MemoryStream(buffer))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
    
}
