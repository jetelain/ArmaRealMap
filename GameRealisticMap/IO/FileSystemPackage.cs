namespace GameRealisticMap.IO
{
    internal class FileSystemPackage : IPackageReader, IPackageWriter
    {
        private readonly string basePath;

        public FileSystemPackage(string basePath)
        {
            this.basePath = basePath;

            Directory.CreateDirectory(basePath);
        }

        public Stream CreateFile(string filename)
        {
            return File.Create(Path.Combine(basePath, filename));
        }

        public Stream ReadFile(string filename)
        {
            return File.OpenRead(Path.Combine(basePath, filename));
        }
    }
}
