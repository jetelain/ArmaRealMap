using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Edit
{
    public sealed class MaterialUpdateGenerator
    {
        private readonly IProgressSystem progess;
        private readonly IGameFileSystemWriter gameFileSystemWriter;

        public MaterialUpdateGenerator(IProgressSystem progress, IGameFileSystemWriter gameFileSystemWriter)
        {
            this.progess = progress;
            this.gameFileSystemWriter = gameFileSystemWriter;
        }

        public void Generate(TerrainMaterialLibrary materialLibrary, IArma3MapConfig config)
        {
            MaterialConfigGenerator.GenerateConfigFiles(gameFileSystemWriter, config, materialLibrary);

            TerrainMaterialHelper.UnpackEmbeddedFiles(materialLibrary, progess, gameFileSystemWriter, config);
        }
    }
}
