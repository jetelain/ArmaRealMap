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

        public static async Task CopyAsync(this IGameFileSystemWriter writer, string sourcePath, string targetPath)
        {
            using var sourceStream = writer.OpenFileIfExists(sourcePath) ?? throw new FileNotFoundException($"File '{sourcePath}' was not found.");
            writer.CreateDirectory(Path.GetDirectoryName(targetPath)!);
            using var targetStream = writer.Create(targetPath);
            await sourceStream.CopyToAsync(targetStream);
        }
    }
}
