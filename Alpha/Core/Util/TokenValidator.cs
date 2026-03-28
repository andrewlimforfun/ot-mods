using System;
using System.Security.Cryptography;
using System.Text;

namespace Alpha.Core.Util
{
    /// <summary>
    /// Validates a plaintext token against an embedded SHA-256 hash.
    /// The hash is supplied at construction time so each mod owns its own secret.
    /// <example>
    /// To generate a hash in PowerShell:
    /// <code>
    /// [System.BitConverter]::ToString(
    ///   [System.Security.Cryptography.SHA256]::Create().ComputeHash(
    ///     [System.Text.Encoding]::UTF8.GetBytes("yourtoken")
    ///   )).Replace("-","").ToLower()
    /// </code>
    /// </example>
    /// </summary>
    public sealed class TokenValidator
    {
        private readonly string _secretHash;

        /// <param name="secretHash">
        /// Lowercase hex SHA-256 of the accepted token.
        /// Pass an empty string to disable the guard — all tokens are accepted.
        /// </param>
        public TokenValidator(string secretHash)
        {
            _secretHash = secretHash;
        }

        /// <summary>
        /// Returns true if <paramref name="token"/> hashes to the embedded secret,
        /// or if the guard is disabled (empty secret hash).
        /// </summary>
        public bool IsValid(string? token)
        {
            if (string.IsNullOrEmpty(_secretHash)) return true;
            if (string.IsNullOrEmpty(token)) return false;

            using var sha = SHA256.Create();
            byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            string hash = BitConverter.ToString(bytes).Replace("-", "").ToLower();
            return hash == _secretHash;
        }
    }
}
