using System.Security.Cryptography;

namespace RansauSysteme.Utils
{
    /// <summary>
    /// Provides utility methods for database operations, including password generation and validation.
    /// </summary>
    public static class DatabaseUtils
    {
        /// <summary>
        /// Generates a secure random password of the specified length with at least one uppercase letter,
        /// one lowercase letter, one digit, and one special character.
        /// </summary>
        /// <param name="length">The desired length of the password. Must be at least 4.</param>
        /// <returns>A string containing the randomly generated password.</returns>
        /// <exception cref="ArgumentException">Thrown when length is less than 4.</exception>
        public static string GeneratePassword(int length = 10)
        {
            if (length < 4)
                throw new ArgumentException("Password length must be at least 4 characters", nameof(length));

            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            // Ensure we have at least one of each character type
            char[] password = new char[length];
            password[0] = ExtractRandomChar(upperCase);
            password[1] = ExtractRandomChar(lowerCase);
            password[2] = ExtractRandomChar(digits);
            password[3] = ExtractRandomChar(special);

            // Fill the rest with mixed characters
            string allChars = upperCase + lowerCase + digits + special;
            for (int i = 4; i < length; i++)
            {
                password[i] = ExtractRandomChar(allChars);
            }

            // Shuffle the password characters
            return ShuffleString(new string(password));
        }

        /// <summary>
        /// Extracts a random character from the provided input string.
        /// </summary>
        /// <param name="input">The string from which to extract a character.</param>
        /// <returns>A randomly selected character from the input string.</returns>
        private static char ExtractRandomChar(string input)
        {
            int random = RandomNumberGenerator.GetInt32(input.Length);
            return input[random];
        }

        /// <summary>
        /// Shuffles the characters in a string to randomize their positions.
        /// </summary>
        /// <param name="input">The string to shuffle.</param>
        /// <returns>A new string with the characters in randomized positions.</returns>
        private static string ShuffleString(string input)
        {
            char[] array = input.ToCharArray();
            int n = array.Length;

            for (int i = n - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }

            return new string(array);
        }

        /// <summary>
        /// Validates if a password meets the specified security requirements.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <param name="minPasswordLength">The minimum required length of the password.</param>
        /// <param name="requireUppercase">Whether an uppercase letter is required.</param>
        /// <param name="requireLowercase">Whether a lowercase letter is required.</param>
        /// <param name="requireDigit">Whether a digit is required.</param>
        /// <param name="requireSpecialChar">Whether a special character is required.</param>
        /// <returns>True if the password meets all specified requirements; otherwise, false.</returns>
        public static bool IsValidPassword(
            string password,
            int minPasswordLength = 8,
            bool requireUppercase = true,
            bool requireLowercase = true,
            bool requireDigit = true,
            bool requireSpecialChar = false)
        {
            if (string.IsNullOrEmpty(password) || password.Length < minPasswordLength)
                return false;

            const string specialChars = @"!@#$%^&*()_+-=[]{}|;:,.<>?/\";

            bool hasUppercase = !requireUppercase || password.Any(char.IsUpper);
            bool hasLowercase = !requireLowercase || password.Any(char.IsLower);
            bool hasDigit = !requireDigit || password.Any(char.IsDigit);
            bool hasSpecialChar = !requireSpecialChar || password.Any(c => specialChars.Contains(c));

            return hasUppercase && hasLowercase && hasDigit && hasSpecialChar;
        }

        /// <summary>
        /// Validates if a password meets the security requirements for the specified strength level.
        /// </summary>
        /// <param name="password">The password to validate.</param>
        /// <param name="strength">The required password strength level.</param>
        /// <param name="minPasswordLength">The minimum required length of the password.</param>
        /// <returns>True if the password meets all requirements for the specified strength; otherwise, false.</returns>
        public static bool IsValidPassword(
            string password,
            PasswordStrength strength = PasswordStrength.Strong,
            int minPasswordLength = 8)
        {
            return strength switch
            {
                PasswordStrength.Basic => IsValidPassword(password, minPasswordLength, false, false, false, false),
                PasswordStrength.Medium => IsValidPassword(password, minPasswordLength, true, true, false, false),
                PasswordStrength.Strong => IsValidPassword(password, minPasswordLength, true, true, true, false),
                PasswordStrength.VeryStrong => IsValidPassword(password, minPasswordLength, true, true, true, true),
                _ => throw new ArgumentOutOfRangeException(nameof(strength))
            };
        }
    }
}