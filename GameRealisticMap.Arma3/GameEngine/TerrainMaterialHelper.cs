using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using Pmad.ProgressTracking;
using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.GameEngine
{
    internal static class TerrainMaterialHelper
    {
        public static void UnpackEmbeddedFiles(TerrainMaterialLibrary materialLibrary, IProgressScope progress, IGameFileSystemWriter gameFileSystemWriter, IArma3MapConfig context)
        {
            foreach(var definition in materialLibrary.Definitions.Where(d => d.Data != null).WithProgress(progress, "Embedded Textures"))
            {
                var data = definition.Data!;

                Unpack(gameFileSystemWriter, data.Format, definition.Material.GetNormalTexturePath(context), data.NormalTexture);

                Unpack(gameFileSystemWriter, data.Format, definition.Material.GetColorTexturePath(context), data.ColorTexture);
            }
        }

        private static void Unpack(IGameFileSystemWriter gameFileSystemWriter, TerrainMaterialDataFormat format, string path, byte[] bytes)
        {
            gameFileSystemWriter.CreateDirectory(Path.GetDirectoryName(path)!);
            if (format == TerrainMaterialDataFormat.PAA)
            {
                using (var stream = gameFileSystemWriter.Create(path))
                {
                    stream.Write(bytes);
                }
            }
            else if (format == TerrainMaterialDataFormat.PNG)
            {
                using var png = Image.Load(bytes);
                gameFileSystemWriter.WritePngImage(Path.ChangeExtension(path,".png"), png);
            }
        }
    }
}
