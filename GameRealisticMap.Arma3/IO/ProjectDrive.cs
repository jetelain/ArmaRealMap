namespace GameRealisticMap.Arma3.IO
{
    public class ProjectDrive : IGameFileSystem
    {
        private readonly string mountPath;
        private readonly IGameFileSystem? autoUnpackFrom;

        public ProjectDrive(string mountPath = "P:", IGameFileSystem? autoUnpackFrom = null)
        {
            this.mountPath = mountPath;
            this.autoUnpackFrom = autoUnpackFrom;
        }

        public bool Exists(string path)
        {
            var fullPath = Path.Combine(mountPath, path);

            return File.Exists(fullPath) || AutoUnpack(path, fullPath);
        }

        private bool AutoUnpack(string path, string fullPath)
        {
            if (path.StartsWith("temp/") || path.StartsWith("temp\\"))
            {
                return false;
            }
            using (var fallBack = autoUnpackFrom?.OpenFileIfExists(path))
            {
                if (fallBack != null)
                {
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
            var fullPath = Path.Combine(mountPath, path);
            if (File.Exists(fullPath) || AutoUnpack(path, fullPath))
            {
                return File.OpenRead(fullPath);
            }
            return null;
        }
    }
}
