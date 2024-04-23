using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using BIS.WRP;
using GameRealisticMap.Arma3.IO;

namespace GameRealisticMap.Arma3.Edit.Imagery
{
    public static class IdMapHelper
    {
        internal static readonly Regex NormalMatch = new Regex(@"texture=""([^""]*)"";\r?\n\ttexGen=1;", RegexOptions.CultureInvariant);

        internal static readonly Regex TextureMatch = new Regex(@"texture=""([^""]*)"";\r?\n\ttexGen=2;", RegexOptions.CultureInvariant);

        public static Task<List<string>> GetUsedTextureList(EditableWrp wrp, ProjectDrive projectDrive)
        {
            return GetUsedTextureList(GetRvMatList(wrp), projectDrive);
        }

        public static List<string> GetRvMatList(EditableWrp wrp)
        {
            return wrp.MatNames
                            .Where(m => !string.IsNullOrEmpty(m))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();
        }

        public async static Task<List<string>> GetUsedTextureList(List<string> rvmat, ProjectDrive projectDrive)
        {
            var textures = new ConcurrentQueue<string>();
            await Parallel.ForEachAsync(rvmat, async (rv, ct) =>
            {
                using var file  = projectDrive.OpenFileIfExists(rv);
                if (file != null)
                {
                    var content = await new StreamReader(file).ReadToEndAsync();
                    foreach (var texture in TextureMatch.Matches(content).Select(m => m.Groups[1].Value))
                    {
                        textures.Enqueue(texture);
                    }
                }
            });
            return textures.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
    }
}
