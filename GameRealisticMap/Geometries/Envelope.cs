namespace GameRealisticMap.Geometries
{
    internal class Envelope : ITerrainEnvelope
    {
        public Envelope(TerrainPoint minPoint, TerrainPoint maxPoint)
        {
            MinPoint = minPoint;
            MaxPoint = maxPoint;
        }

        public TerrainPoint MinPoint { get; }

        public TerrainPoint MaxPoint { get; }
    }
}
