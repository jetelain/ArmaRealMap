namespace GameRealisticMap.Geometries
{
    internal class Envelope : ITerrainEnvelope
    {
        internal static readonly Envelope None = new Envelope(new TerrainPoint(float.MinValue, float.MinValue), new TerrainPoint(float.MaxValue, float.MaxValue));

        public Envelope(TerrainPoint minPoint, TerrainPoint maxPoint)
        {
            MinPoint = minPoint;
            MaxPoint = maxPoint;
        }

        public TerrainPoint MinPoint { get; }

        public TerrainPoint MaxPoint { get; }
    }
}
