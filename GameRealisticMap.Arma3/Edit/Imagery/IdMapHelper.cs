using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using BIS.WRP;
using GameRealisticMap.Arma3.IO;

namespace GameRealisticMap.Arma3.Edit.Imagery
{
    public static class IdMapHelper
    {
        internal static readonly Regex NormalMatch = new Regex(@"texture\s*=\s*""([^""]*)"";\s*texGen\s*=\s*1;", RegexOptions.CultureInvariant | RegexOptions.Multiline);

        internal static readonly Regex TextureMatch = new Regex(@"texture\s*=\s*""([^""]*)"";\s*texGen\s*=\s*2;", RegexOptions.CultureInvariant | RegexOptions.Multiline);

        public static Task<List<GroundDetailTexture>> GetUsedTextureList(EditableWrp wrp, IGameFileSystem projectDrive)
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

        public async static Task<List<GroundDetailTexture>> GetUsedTextureList(List<string> rvmat, IGameFileSystem projectDrive)
        {
            var textures = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            await Parallel.ForEachAsync(rvmat, async (rv, ct) =>
            {
                using var file  = projectDrive.OpenFileIfExists(rv);
                if (file != null)
                {
                    var content = await new StreamReader(file).ReadToEndAsync();
                    var colors = TextureMatch.Matches(content).Select(m => m.Groups[1].Value).ToList();
                    var normals = NormalMatch.Matches(content).Select(m => m.Groups[1].Value).ToList();
                    if (colors.Count == normals.Count)
                    {
                        for( var i = 0; i < colors.Count; i++ )
                        {
                            textures.TryAdd(colors[i], normals[i]);
                        }
                    }
                }
            });
            return textures.Select(p => new GroundDetailTexture(p.Key, p.Value)).ToList();
        }
    }
}
