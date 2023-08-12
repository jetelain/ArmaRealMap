namespace GameRealisticMap.Arma3.Edit
{
    public class WrpSetElevationGrid
    {
        public WrpSetElevationGrid(int x, int y, float elevation)
        {
            Elevation = elevation;
            Y = y;
            X = x;
        }

        public float Elevation { get; }
        public int Y { get; }
        public int X { get; }
    }
}