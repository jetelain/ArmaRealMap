using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.GameEngine
{
    public class ImageryTile
    {
        public int X { get; }

        public int Y { get; }

        /// <summary>
        /// Coordinates in initial image
        /// </summary>
        public Point ImageTopLeft { get; }

        public Point ImageBottomRight { get; }

        public int Size { get; }

        public double UB { get; }

        public double VB { get; }

        public double UA { get; }

        public ImageryTile(int x, int y, int step, int halfOverlap, int tileSize, int top, double ua)
        {
            X = x;
            Y = y;
            ImageTopLeft = new Point(x * step - halfOverlap, y * step - halfOverlap);
            ImageBottomRight = ImageTopLeft + new Size(tileSize, tileSize);
            Size = tileSize;
            UB = ((double)(halfOverlap - (x * step))) / (double)tileSize;
            VB = ((double)(top - (y * step))) / (double)tileSize;
            UA = ua;
        }

        public bool ContainsImagePoint(Point p)
        {
            return p.X >= ImageTopLeft.X && p.Y >= ImageTopLeft.Y 
                && p.X < ImageBottomRight.X && p.Y < ImageBottomRight.Y;
        }
    }
}
