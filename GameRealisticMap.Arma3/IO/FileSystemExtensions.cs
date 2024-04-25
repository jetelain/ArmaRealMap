namespace GameRealisticMap.Arma3.IO
{
    internal static class FileSystemExtensions
    {
        public static string ReadAllText(this IGameFileSystem gameFileSystem, string path)
        {
            using(var stream = gameFileSystem.OpenFileIfExists(path))
            {
                if (stream != null)
                {
                    return new StreamReader(stream).ReadToEnd();
                }
            }
            throw new FileNotFoundException(null, path);
        }

        public static async Task<string> ReadAllTextAsync(this IGameFileSystem gameFileSystem, string path)
        {
            using (var stream = gameFileSystem.OpenFileIfExists(path))
            {
                if (stream != null)
                {
                    return await new StreamReader(stream).ReadToEndAsync();
                }
            }
            throw new FileNotFoundException(null, path);
        }
    }
}
