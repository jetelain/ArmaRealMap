using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace ArmaRealMap.TerrainData.GroundDetailTextures
{
    public class TerrainTiler
    {
        public LandSegment[,] Segments { get; }

        public IEnumerable<LandSegment> All
        {
            get
            {
                foreach (var segment in Segments)
                {
                    yield return segment;
                }
            }
        }

        public TerrainTiler(Config config) 
            : this(MapInfos.Create(config), config)
        {

        }

        public TerrainTiler(MapInfos area, Config config)
        {
            var step = config.TileSize - (config.RealTileOverlap * 2);
            var num = (int)Math.Ceiling((double)area.ImageryWidth / (double)step);
            var top = area.ImageryHeight + config.TileSize - config.RealTileOverlap - step;
            var ua =  (1d / config.Resolution.Value) / config.TileSize;
            Segments = new LandSegment[num,num];
            for (int x = 0; x < num; x++)
            {
                for (int y = 0; y < num; y++)
                {
                    Segments[x,y] = new LandSegment(x, y, step, config.RealTileOverlap, config.TileSize, top, ua);
                }
            }
        }
    }

    public class LandSegment
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

        public LandSegment(int x, int y, int step, int realTileOverlap, int tileSize, int top, double ua)
        {
            X = x;
            Y = y;
            ImageTopLeft = new Point(x * step - realTileOverlap, y * step - realTileOverlap);
            ImageBottomRight = ImageTopLeft + new Size(tileSize, tileSize);
            Size = tileSize;
            UB = ((double)(realTileOverlap - (x * step))) / (double)tileSize;
            VB = ((double)(top - (y * step))) / (double)tileSize;
            UA = ua;
        }

        public bool ContainsImagePoint(Point p)
        {
            return p.X >= ImageTopLeft.X && p.Y >= ImageTopLeft.Y 
                && p.X <= ImageBottomRight.X && p.Y <= ImageBottomRight.Y;
        }
    }

}
