using GameRealisticMap;

namespace DatasetsLoader
{
    public sealed class DatasetMap
    {
        public DatasetMap(string name, ITerrainArea terrainArea)
        {
            Name = name;
            TerrainArea = terrainArea;
        }

        public string Name { get; }

        public ITerrainArea TerrainArea { get; }
    }
}