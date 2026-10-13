using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Security.Cryptography;

namespace Finance_App
{
    public static class PasswordHelper
    {
        public static string GenerateSalt(int size = 16)
        {
            byte[] buffer = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }
            return Convert.ToBase64String(buffer);
        }

        public static string ComputeHash(string password, string salt) 
        {
            using (var sha = SHA256.Create()) 
            {
                var combined = password + salt;
                byte[] bytes = Encoding.UTF8.GetBytes(combined);
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
