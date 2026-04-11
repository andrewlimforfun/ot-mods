using System;

namespace Alpha.Core.Util
{
    /// <summary>
    /// Compile-time list of trusted player Steam IDs, stored in obfuscated form.
    /// Use <see cref="IsAdmin"/> to check membership at runtime.
    /// </summary>
    public static class ModAdmins
    {
        private static readonly string[] _obfuscatedIds =
        {
            /*Alpha*/ "lMH354nRrVmSw/blgdmtUpM=",
            /*Beta*/  "lMH354nRrVmUxfvkgNitWZA=",
        };

        /// <summary>Returns true if the given Steam ID64 is in the trusted list.</summary>
        public static bool IsAdmin(string steamId)
        {
            if (string.IsNullOrEmpty(steamId)) return false;
            foreach (string entry in _obfuscatedIds)
                if (StringObfuscator.Deobfuscate(entry) == steamId)
                    return true;
            return false;
        }
    }
}
