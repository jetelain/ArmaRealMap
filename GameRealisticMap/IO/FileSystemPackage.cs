namespace GameRealisticMap.IO
{
    public sealed class FileSystemPackage : IPackageReader, IPackageWriter
    {
        private readonly string basePath;

        public FileSystemPackage(string basePath)
        {
            this.basePath = basePath;

            Directory.CreateDirectory(basePath);
        }

        public Stream CreateFile(string filename)
        {
            var target = Path.Combine(basePath, filename);
            var targetDirectory = Path.GetDirectoryName(target);
            if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory)) 
            {
                Directory.CreateDirectory(targetDirectory);
            }
            return File.Create(Path.Combine(basePath, filename));
        }

        public Stream ReadFile(string filename)
        {
            return File.OpenRead(Path.Combine(basePath, filename));
        }
    }
}
