using System;
using System.Linq;
using BIS.WRP;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal static class LicensingGuard
    {
        private static readonly string[] ProtectedPrefixes = ["a3\\"];

        public static bool IsProtected(EditableWrp? world)
        {
            // Maps within 'a3\' are licensed with a restrictive EULA : will deny to do anything with those maps
            // Test materials path, as it's easy to move or rename wrp file, but it's hard to change materials (without a wrp editor)
            return world != null && world.MatNames.Where(m => m != null).Any(IsProtectedPath);
        }

        private static bool IsProtectedPath(string path)
        {
            return ProtectedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }
    }
}
