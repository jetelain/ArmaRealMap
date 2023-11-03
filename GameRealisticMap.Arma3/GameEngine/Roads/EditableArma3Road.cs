using GameRealisticMap.Geometries;

namespace GameRealisticMap.Arma3.GameEngine.Roads
{
    public sealed class EditableArma3Road : IArma3Road
    {
        public EditableArma3Road(int order, EditableArma3RoadTypeInfos roadTypeInfos, TerrainPath path)
        {
            Order = order;
            RoadTypeInfos = roadTypeInfos;
            Path = path;
        }

        public EditableArma3RoadTypeInfos RoadTypeInfos { get; set; }

        public TerrainPath Path { get; set; }

        public int Order { get; set; }

        IArma3RoadTypeInfos IArma3Road.TypeInfos => RoadTypeInfos;
    }
}
