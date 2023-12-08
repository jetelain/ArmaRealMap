using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using GameRealisticMap.Arma3.IO;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.Services
{
    internal sealed class Arma3BackupFileSystem : IGameFileSystem
    {
        private readonly ZipArchive archive;
        private readonly HashSet<string> files;

        public Arma3BackupFileSystem(ZipArchive archive, IEnumerable<string> files)
        {
            this.archive = archive;
            this.files = new HashSet<string>(files, StringComparer.OrdinalIgnoreCase);
        }

        public IEnumerable<string> FindAll(string pattern)
        {
            return Enumerable.Empty<string>();
        }

        public DateTime? GetLastWriteTimeUtc(string path)
        {
            if (files.Contains(path))
            {
                var entry = archive.GetEntry(Path.GetFileName(path));
                if (entry != null)
                {
                    return entry.LastWriteTime.UtcDateTime;
                }
            }
            return null;
        }

        public Stream? OpenFileIfExists(string path)
        {
            if (files.Contains(path))
            {
                var entry = archive.GetEntry(Path.GetFileName(path));
                if (entry != null)
                {
                    var mem = new MemoryStream();
                    using(var stream = entry.Open())
                    {
                        stream.CopyTo(mem);
                    }
                    mem.Position = 0;
                    return mem;
                }
            }
            return null;
        }
    }
}
