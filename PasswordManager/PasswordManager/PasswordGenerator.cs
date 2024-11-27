using System;
using System.Linq;

namespace PasswordManager
{
    public static class PasswordGenerator
    {
        public static string GeneratePassword(int length, string allowedChars)
        {
            var random = new Random();
            return new string(Enumerable.Repeat(allowedChars, length)
                                        .Select(chars => chars[random.Next(chars.Length)]).ToArray());
        }
    }
}