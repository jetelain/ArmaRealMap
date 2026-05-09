using System.Diagnostics.CodeAnalysis;

namespace GameRealisticMap.Arma3
{
    /// <summary>
    /// Provides utility methods for working with Arma 3 file paths, to ensure consistent behavior across different platforms and path formats.
    /// </summary>
    public static class Arma3Path
    {
        /// <summary>
        /// Return the file name without extension, with both types of slashes supported, and null if path is null
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The file name without extension, or null if path is null.</returns>
        [return: NotNullIfNotNull(nameof(path))]
        public static string? GetFileNameWithoutExtension(string? path)
        {
            return Path.GetFileNameWithoutExtension(path?.Replace('\\', '/'));
        }
    }
}
