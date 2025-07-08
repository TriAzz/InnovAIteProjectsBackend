using System;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace innovaite_projects_dashboard.Authentication
{
    public static class Argon2PasswordHasher
    {
        private const int DegreeOfParallelism = 8;
        private const int Iterations = 4;
        private const int MemorySize = 1024 * 1024; // 1 GB

        public static string HashPassword(string password)
        {
            var salt = CreateSalt();
            var hash = HashPassword(password, salt);
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var parts = hashedPassword.Split(':');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);
            var newHash = HashPassword(password, salt);

            return hash.SequenceEqual(newHash);
        }

        private static byte[] CreateSalt()
        {
            var buffer = new byte[16];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(buffer);
            return buffer;
        }

        private static byte[] HashPassword(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = DegreeOfParallelism;
            argon2.Iterations = Iterations;
            argon2.MemorySize = MemorySize;

            return argon2.GetBytes(16);
        }
    }
}
