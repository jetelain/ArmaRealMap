namespace GameRealisticMap.Geometries
{
    internal class MutableTerrainPath
    {
        public MutableTerrainPath(List<TerrainPoint> points)
        {
            this.Points = points;
        }

        public MutableTerrainPath(TerrainPath path)
            : this(path.Points.ToList())
        {

        }

        public List<TerrainPoint> Points { get; }

        public TerrainPoint FirstPoint => Points[0];

        public TerrainPoint LastPoint => Points[Points.Count - 1];

        public float Length => TerrainPath.GetLength(Points);

        public bool IsClosed => FirstPoint.Equals(LastPoint);

        public TerrainPath ToPath()
        {
            return new TerrainPath(Points);
        }

    }
}
