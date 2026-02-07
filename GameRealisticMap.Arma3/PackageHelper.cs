using GameRealisticMap.Arma3.IO;
using GameRealisticMap.IO;

namespace GameRealisticMap.Arma3
{
    internal static class PackageHelper
    {
        internal static IPackageWriter? GetPackageWriter(Arma3MapConfig a3config, ProjectDrive projectDrive)
        {
            if (a3config.IsPersisted)
            {
                return new FileSystemPackage(Path.Combine(projectDrive.GetFullPath(a3config.PboPrefix), ".grm"));
            }
            return null;
        }
    }
}
