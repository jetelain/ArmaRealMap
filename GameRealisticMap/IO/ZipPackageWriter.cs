using System.IO.Compression;

namespace GameRealisticMap.IO
{
    internal class ZipPackageWriter : IPackageWriter
    {
        private readonly ZipArchive archive;

        public ZipPackageWriter(ZipArchive archive)
        {
            this.archive = archive;
        }

        public Stream CreateFile(string filename)
        {
            return archive.CreateEntry(filename).Open();
        }
    }
}