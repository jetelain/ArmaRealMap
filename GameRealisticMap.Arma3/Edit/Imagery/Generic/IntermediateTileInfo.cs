using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.Edit.Imagery.Generic
{
    internal class IntermediateTileInfo
    {
        public required string Sat { get; set; }
        public required string Mask { get; set; }
        public required List<string> Textures { get; set; }
        public required List<string> Normals { get; set; }
        public Rectangle CellIndexes { get; internal set; }
        public float UA { get; internal set; }
        public float UB { get; internal set; }
        public float VB { get; internal set; }
    }
}