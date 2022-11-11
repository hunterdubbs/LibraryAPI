using LibraryAPI.Domain;
using System;
using System.Security.Cryptography;
using System.Text;

namespace LibraryAPI.LogicProcessors
{
    public static class SecurityHelper
    {
        private static readonly string codeCharSpace = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string GenerateAlphanumericCode(int length)
        {
            StringBuilder result = new StringBuilder();
            Random random = new Random();

            for (int i = 0; i < length; i++){
                result.Append(codeCharSpace[random.Next(0, codeCharSpace.Length - 1)]);
            }

            return result.ToString();
        }

        public static SaltedHash GenerateSaltedHash(string input)
        {
            var resultBytes = new Rfc2898DeriveBytes(input, 256, 2048);
            resultBytes.Reset();

            var salt = resultBytes.Salt;
            var hash = resultBytes.GetBytes(256);

            return new SaltedHash()
            {
                Hash = Convert.ToBase64String(hash),
                Salt = Convert.ToBase64String(salt)
            };
        }

        public static bool CompareToSaltedHash(string candidate, SaltedHash saltedHash)
        {
            var hash = Convert.FromBase64String(saltedHash.Hash);
            var salt = Convert.FromBase64String(saltedHash.Salt);

            var resultBytes = new Rfc2898DeriveBytes(candidate, salt, 2048);
            resultBytes.Reset();
            var newHash = resultBytes.GetBytes(256);

            if (hash.Length != newHash.Length) return false;

            for(int i = 0; i < hash.Length; i++)
            {
                if (hash[i] != newHash[i]) return false;
            }

            return true;
        }
    }
}
