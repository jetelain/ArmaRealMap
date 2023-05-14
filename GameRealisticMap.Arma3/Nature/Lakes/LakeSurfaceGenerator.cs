using System.Numerics;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3.Nature.Lakes
{
    public class LakeSurfaceGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IArma3RegionAssets assets;

        public LakeSurfaceGenerator(IArma3RegionAssets assets)
        {
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context)
        {
            var lakes = context.GetData<ElevationWithLakesData>().Lakes;
            var result = new List<TerrainBuilderObject>();
            var minimalPondSize = PondSizeId.Size5;
            foreach (var lake in lakes)
            {
                GenerateTiles(result, lake.TerrainPolygon, lake.WaterElevation, minimalPondSize);
            }
            return result;
        }

        private void GenerateTiles(List<TerrainBuilderObject> result, TerrainPolygon polygon, float waterElevation, PondSizeId minimalPondSize)
        {
            var minimalTileSize = (int)minimalPondSize;
            var min = polygon.MinPoint.Vector - new Vector2(0.5f);
            var size = (polygon.MaxPoint.Vector - min + new Vector2(0.5f)) / minimalTileSize;
            var w10 = (int)Math.Ceiling(size.X) + 1;
            var h10 = (int)Math.Ceiling(size.Y) + 1;
            var grid10 = GenerateMap(polygon, min, w10, h10, minimalTileSize);
            Process(result, grid10, w10, h10, min, PondSizeId.Size80, waterElevation, minimalTileSize);
            Process(result, grid10, w10, h10, min, PondSizeId.Size40, waterElevation, minimalTileSize);
            Process(result, grid10, w10, h10, min, PondSizeId.Size20, waterElevation, minimalTileSize);
            Process(result, grid10, w10, h10, min, PondSizeId.Size10, waterElevation, minimalTileSize);
            Process(result, grid10, w10, h10, min, PondSizeId.Size5, waterElevation, minimalTileSize);
        }

        private static bool[,] GenerateMap(TerrainPolygon polygon, Vector2 min, int w10, int h10, int minimalTileSize)
        {
            var grid10 = new bool[w10, h10];
            using (var img = new Image<Rgba32>(w10, h10, Color.Transparent))
            {
                img.Mutate(d => PolygonDrawHelper.DrawPolygon(d, polygon, new SolidBrush(Color.White), l => l.Select(p => (PointF)((p.Vector - min) / minimalTileSize))));
                for (int x = 0; x < w10; ++x)
                {
                    for (int y = 0; y < h10; ++y)
                    {
                        grid10[x, y] = img[x, y].A > 0;
                    }
                }
            }
            return grid10;
        }

        private void Process(List<TerrainBuilderObject> objects, bool[,] grid10, int w10, int h10, Vector2 min, PondSizeId pondSize, float waterElevation, int minimalTileSize)
        {
            var model = assets.GetPond(pondSize);
            var objSize = ((int)pondSize) / minimalTileSize;
            var hsize = ((int)pondSize) / 2f;
            for (int x = 0; x < w10; x += objSize)
            {
                for (int y = 0; y < h10; y += objSize)
                {
                    if (x + objSize <= w10 && y + objSize < h10 && Match(grid10, x, y, x + objSize, y + objSize))
                    {
                        Take(grid10, x, y, x + objSize, y + objSize);
                        var ax = x * minimalTileSize + hsize;
                        var ay = y * minimalTileSize + hsize;
                        objects.Add(new TerrainBuilderObject(model, new TerrainPoint(min.X + ax, min.Y + ay), waterElevation, ElevationMode.Absolute, 0, 0, 0, 1));
                    }
                }
            }
        }

        private static void Take(bool[,] grid10, int minX, int minY, int maxX, int maxY)
        {
            for (int x = minX; x < maxX; ++x)
            {
                for (int y = minY; y < maxY; ++y)
                {
                    grid10[x, y] = false;
                }
            }
        }

        private static bool Match(bool[,] grid10, int minX, int minY, int maxX, int maxY)
        {
            for (int x = minX; x < maxX; ++x)
            {
                for (int y = minY; y < maxY; ++y)
                {
                    if (!grid10[x, y])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

    }
}
