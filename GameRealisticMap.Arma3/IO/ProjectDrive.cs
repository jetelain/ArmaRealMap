using GameRealisticMap.Reporting;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.IO
{
    public class ProjectDrive : IGameFileSystem, IGameFileSystemWriter
    {
        private readonly string mountPath;
        private readonly IGameFileSystem? secondarySource;
        private readonly List<string> imageToPaaPending = new ();
        private readonly List<KeyValuePair<string, string>> mountPoints = new ();

        public ProjectDrive(string mountPath = "P:", IGameFileSystem? secondarySource = null)
        {
            this.mountPath = mountPath;
            this.secondarySource = secondarySource;
        }

        public string MountPath => mountPath;

        public IGameFileSystem? SecondarySource => secondarySource;

        public string GetFullPath(string path)
        {
            if (path.StartsWith("\\", StringComparison.Ordinal))
            {
                path = path.Substring(1);
            }
            foreach(var item in mountPoints)
            {
                if (path.StartsWith(item.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return Path.Combine(item.Value, path.Substring(item.Key.Length));
                }
            }
            // Should call Arma3ToolsHelper.EnsureProjectDrive() if mountPath == "P:" on first call
            return Path.Combine(mountPath, path);
        }

        public bool EnsureLocalFileCopy(string path)
        {
            var fullPath = GetFullPath(path);
            if (File.Exists(fullPath))
            {
                return true;
            }
            using (var fallBack = secondarySource?.OpenFileIfExists(path))
            {
                if (fallBack != null)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                    using (var target = File.Create(fullPath))
                    {
                        fallBack.CopyTo(target);
                    }
                    return true;
                }
            }
            return false;
        }

        public Stream? OpenFileIfExists(string path)
        {
            var fullPath = GetFullPath(path);
            if (File.Exists(fullPath))
            {
                return File.OpenRead(fullPath);
            }
            return secondarySource?.OpenFileIfExists(path);
        }

        public void CreateDirectory(string path)
        {
            Directory.CreateDirectory(GetFullPath(path));
        }

        public void WritePngImage(string path, Image image)
        {
            var fullPath = GetFullPath(path);
            image.SaveAsPng(fullPath);
            imageToPaaPending.Add(fullPath);
        }

        public void WriteTextFile(string path, string text)
        {
            File.WriteAllText(GetFullPath(path), text);
        }

        public Stream Create(string path)
        {
            return File.Create(GetFullPath(path));
        }

        public async Task ProcessImageToPaa(IProgressSystem progress, int? maxDegreeOfParallelism = null)
        {
            await Arma3ToolsHelper.ImageToPAA(progress, imageToPaaPending, maxDegreeOfParallelism);
            imageToPaaPending.Clear();
        }

        public void AddMountPoint(string gamePath, string physicalPath)
        {
            if (!gamePath.EndsWith("\\", StringComparison.Ordinal))
            {
                gamePath = gamePath + "\\";
            }
            mountPoints.Add(new (gamePath, physicalPath));
            mountPoints.Sort((a, b) => b.Key.Length.CompareTo(a.Key.Length));
        }

        public bool FileExists(string path)
        {
            return File.Exists(GetFullPath(path));
        }

        public string GetGamePath(string fullPath)
        {
            if (fullPath.StartsWith("P:\\", StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(3);
            }
            if (fullPath.StartsWith(mountPath, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(mountPath.Length).TrimStart('\\');
            }
            foreach (var item in mountPoints)
            {
                if (fullPath.StartsWith(item.Value, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Key + fullPath.Substring(item.Value.Length);
                }
            }
            return fullPath;
        }

        public IEnumerable<string> FindAll(string pattern)
        {
            var physical = Directory.GetFiles(mountPath, pattern, SearchOption.AllDirectories)
                .Select(file => file.Substring(mountPath.Length).TrimStart('\\'))
                .Where(file => !file.StartsWith("temp\\", StringComparison.OrdinalIgnoreCase));
            if (secondarySource != null)
            {
                return secondarySource.FindAll(pattern).Concat(physical).Distinct(StringComparer.OrdinalIgnoreCase);
            }
            return physical;
        }
    }
}
