using System.Diagnostics;
using System.Runtime.Versioning;
using GameRealisticMap.Arma3.Assets;

namespace GameRealisticMap.Arma3.GameLauncher
{
    public static class Arma3Helper
    {
        [SupportedOSPlatform("windows")]
        public static void Launch(List<ModDependencyDefinition> dependencies, string localModpath, string worldName)
        {
            var arma3 = Arma3ToolsHelper.GetArma3Path();
            var workshop = Arma3ToolsHelper.GetArma3WorkshopPath();

            var mods = string.Join(";", dependencies.Select(d => Path.Combine(workshop, d.SteamId)).Concat(new[] { localModpath }));

            Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = false,
                FileName = Path.Combine(arma3, "Arma3_x64.exe"),
                WorkingDirectory = arma3,
                Arguments = $"\"-world={worldName}\" \"-mod={mods}\""
            });
        }
    }
}
