namespace Hush.Core
{
    /// <summary>
    /// Lightweight static settings for Hush that can be read without loading
    /// <see cref="HushPlugin"/> (which inherits MonoBehaviour).
    /// </summary>
    public static class HushSettings
    {
        public static bool VerboseLogging { get; set; }
    }
}
