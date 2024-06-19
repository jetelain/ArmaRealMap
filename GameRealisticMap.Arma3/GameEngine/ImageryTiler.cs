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

        public int TileOverlap { get; }

        public int IdMapMultiplier { get; }

        public int IdMapTileSize => TileSize * IdMapMultiplier;

        public Size IdMapFullImageSize => FullImageSize * IdMapMultiplier;

        public ImageryTiler(IArma3MapConfig config)
            : this(config.TileSize, config.Resolution, config.SizeInMeters, config.IdMapMultiplier)
        {

        }

        public ImageryTiler(int tileSize, double resolution, float sizeInMeters, int idMapMultiplier = 1)
        {
            TileSize = tileSize;
            FullImageSize = new Size((int)Math.Ceiling(sizeInMeters / resolution));
            Resolution = resolution;
            IdMapMultiplier = idMapMultiplier;

            if (idMapMultiplier * tileSize >  4096)
            {
                throw new ArgumentException($"Id map tile would be too big: {idMapMultiplier} * {tileSize} => {idMapMultiplier * tileSize}");
            }

            // https://github.com/pennyworth12345/A3_MMSI/wiki/Mapframe-Information

            // Real tile overlap computation
            var textureLayerSize = (double)sizeInMeters / WrpCompiler.LandRange(sizeInMeters);
            var startTileSizePixels = tileSize - 16; // 16 px is the desired overlap
            var startTileSizeMeters = Resolution * startTileSizePixels;
            var landgridCellCount = startTileSizeMeters / textureLayerSize;
            landgridCellCount -= landgridCellCount % 4;
            var tileSizeMeters = landgridCellCount * textureLayerSize;
            var tileSizePixels = tileSizeMeters / Resolution;
            TileOverlap = (int)(tileSize - tileSizePixels);

            var halfOverlap = TileOverlap / 2;
            var step = tileSize - TileOverlap;
            var num = (int)Math.Ceiling((double)FullImageSize.Width / (double)step);
            var top = FullImageSize.Height + tileSize - halfOverlap - step;
            var ua =  (1d / resolution) / tileSize;
            Segments = new ImageryTile[num,num];
            for (int x = 0; x < num; x++)
            {
                for (int y = 0; y < num; y++)
                {
                    Segments[x,y] = new ImageryTile(x, y, step, halfOverlap, tileSize, top, ua);
                }
            }
        }
    }

}
