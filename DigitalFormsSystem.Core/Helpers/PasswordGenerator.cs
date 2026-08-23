using System.Security.Cryptography;

namespace DigitalFormsSystem.Core.Helpers
{
    public static class PasswordGenerator
    {
        private static readonly string _uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private static readonly string _lowercase = "abcdefghijklmnopqrstuvwxyz";
        private static readonly string _digits = "0123456789";
        private static readonly string _special = "!@#$%^&*()_-+=?";
        private static readonly string _allChars = _uppercase + _lowercase + _digits + _special;

        public static string GeneratePasswordWithEmployeeNo(string employeeNo, int randomLength = 8)
        {
            // Extract prefix (e.g., HS9501)
            var parts = employeeNo.Split('-');
            var prefix = parts.Length > 0 ? parts[0] : employeeNo;

            // Limit to 6 characters
            if (prefix.Length > 6)
            {
                prefix = prefix.Substring(0, 6);
            }

            // Generate random part
            var randomPart = GenerateRandomString(randomLength);

            // Combine: prefix + @ + random
            return prefix + "@" + randomPart;
        }

        public static string GenerateRandomString(int length)
        {
            if (length < 4) length = 4;

            var result = new char[length];
            var random = RandomNumberGenerator.Create();

            // Ensure at least one of each type
            result[0] = _uppercase[GetRandomInt(random, _uppercase.Length)];
            result[1] = _lowercase[GetRandomInt(random, _lowercase.Length)];
            result[2] = _digits[GetRandomInt(random, _digits.Length)];
            result[3] = _special[GetRandomInt(random, _special.Length)];

            // Fill the rest randomly
            for (int i = 4; i < length; i++)
            {
                result[i] = _allChars[GetRandomInt(random, _allChars.Length)];
            }

            // Shuffle
            return new string(ShuffleArray(result, random));
        }

        private static int GetRandomInt(RandomNumberGenerator rng, int max)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            return Math.Abs(BitConverter.ToInt32(bytes, 0)) % max;
        }

        private static char[] ShuffleArray(char[] array, RandomNumberGenerator rng)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                var j = GetRandomInt(rng, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
            return array;
        }
    }
}