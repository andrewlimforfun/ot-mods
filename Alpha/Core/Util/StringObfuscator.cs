using System;
using System.Text;

namespace Alpha.Core.Util
{
    /// <summary>
    /// Simple XOR + Base64 obfuscation for strings.
    /// Not cryptographically secure — intended to prevent casual plain-text inspection only.
    /// </summary>
    public static class StringObfuscator
    {
        private static readonly byte[] Key = { 0xA3, 0xF7, 0xC2, 0xD1, 0xB8, 0xE0, 0x94, 0x61 };

        /// <summary>Obfuscates a string to a Base64-encoded form.</summary>
        public static string Obfuscate(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] ^= Key[i % Key.Length];
            return Convert.ToBase64String(bytes);
        }

        /// <summary>Reverses <see cref="Obfuscate"/>. Throws <see cref="FormatException"/> if the input is not valid Base64.</summary>
        public static string Deobfuscate(string encoded)
        {
            if (encoded == null) throw new ArgumentNullException(nameof(encoded));
            byte[] bytes = Convert.FromBase64String(encoded);
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] ^= Key[i % Key.Length];
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
