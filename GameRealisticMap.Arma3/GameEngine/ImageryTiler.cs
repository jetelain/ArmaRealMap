using SixLabors.ImageSharp;

namespace GameRealisticMap.Arma3.GameEngine
{
    public class ImageryTiler
    {
        public ImageryTile[,] Segments { get; }

        public IEnumerable<ImageryTile> All
        {
            get
            {
                foreach (var segment in Segments)
                {
                    yield return segment;
                }
            }
        }

        public int TileSize { get; }

        public Size FullImageSize { get; }

        public double Resolution { get; }

        public ImageryTiler(int tileSize, double resolution, Size fullImageSize)
        {
            TileSize = tileSize;
            FullImageSize = fullImageSize;
            Resolution = resolution;

            var realTileOverlap = tileSize / 32;
            var step = tileSize - (realTileOverlap * 2);
            var num = (int)Math.Ceiling((double)fullImageSize.Width / (double)step);
            var top = fullImageSize.Height + tileSize - realTileOverlap - step;
            var ua =  (1d / resolution) / tileSize;
            Segments = new ImageryTile[num,num];
            for (int x = 0; x < num; x++)
            {
                for (int y = 0; y < num; y++)
                {
                    Segments[x,y] = new ImageryTile(x, y, step, realTileOverlap, tileSize, top, ua);
                }
            }
        }
    }

}
