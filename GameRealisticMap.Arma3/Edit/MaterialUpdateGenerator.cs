using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Edit.Imagery;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Reporting;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Edit
{
    public sealed class MaterialUpdateGenerator
    {
        private readonly IProgressScope progess;
        private readonly IGameFileSystemWriter gameFileSystemWriter;

        public MaterialUpdateGenerator(IProgressScope progress, IGameFileSystemWriter gameFileSystemWriter)
        {
            this.progess = progress;
            this.gameFileSystemWriter = gameFileSystemWriter;
        }

        public void Generate(TerrainMaterialLibrary materialLibrary, IArma3MapConfig config)
        {
            MaterialConfigGenerator.GenerateConfigFiles(gameFileSystemWriter, config, materialLibrary);

            TerrainMaterialHelper.UnpackEmbeddedFiles(materialLibrary, progess, gameFileSystemWriter, config);
        }

        public async Task Replace(List<string> rvmat, string sourceColorTexture, TerrainMaterial replacement, IArma3MapConfig context)
        {
            using var task = progess.CreateInteger("ReplaceRvMat", rvmat.Count);
            await Parallel.ForEachAsync(rvmat, async (file, token) => 
            {
                await Replace(file, sourceColorTexture, replacement, context);
                task.ReportOneDone();
            });
        }

        public async ValueTask Replace(string rvmat, string sourceColorTexture, TerrainMaterial replacement, IArma3MapConfig context)
        {
            var content = await gameFileSystemWriter.ReadAllTextAsync(rvmat);

            var normals = IdMapHelper.NormalMatch.Matches(content).ToList();
            var colors = IdMapHelper.TextureMatch.Matches(content).ToList();

            if (normals.Count != colors.Count)
            {
                progess.WriteLine($"File {rvmat} is corrupted. Unable to update.");
                return;
            }
            var changed = false;
            for (int index = colors.Count - 1; index >= 0; index--)
            {
                var colorGroup = colors[index].Groups[1];
                var normalGroup = normals[index].Groups[1];
                if (normalGroup.Index >= colorGroup.Index)
                {
                    progess.WriteLine($"File {rvmat} is corrupted. Unable to update.");
                    return;
                }
                if (string.Equals(colorGroup.Value, sourceColorTexture, StringComparison.OrdinalIgnoreCase))
                {
                    content = content.Substring(0, colorGroup.Index) + replacement.GetColorTexturePath(context) + content.Substring(colorGroup.Index + colorGroup.Length);
                    content = content.Substring(0, normalGroup.Index) + replacement.GetNormalTexturePath(context) + content.Substring(normalGroup.Index + normalGroup.Length);
                    changed = true;
                }
            }
            if (changed)
            {
                gameFileSystemWriter.WriteTextFile(rvmat, content);
            }
        }
    }
}
