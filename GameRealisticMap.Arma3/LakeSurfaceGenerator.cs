using System.Numerics;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3
{
    internal class LakeSurfaceGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IArma3RegionAssets assets;

        public LakeSurfaceGenerator(IArma3RegionAssets assets)
        {
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate (IArma3MapConfig config, IContext context)
        {
            var lakes = context.GetData<ElevationWithLakesData>().Lakes;
            var result = new List<TerrainBuilderObject>();
            foreach(var lake in lakes)
            {
                GenerateTiles(result, lake.TerrainPolygon, lake.WaterElevation);
            }
            return result;
        }

        private void GenerateTiles(List<TerrainBuilderObject> result, TerrainPolygon polygon, float waterElevation)
        {
            var min = polygon.MinPoint.Vector;
            var size = (polygon.MaxPoint.Vector - polygon.MinPoint.Vector) / 10f;
            var w10 = (int)Math.Ceiling(size.X);
            var h10 = (int)Math.Ceiling(size.Y);
            var grid10 = GenerateMap(polygon, min, w10, h10);
            Process(result, grid10, w10, h10, min, 80, waterElevation);
            Process(result, grid10, w10, h10, min, 40, waterElevation);
            Process(result, grid10, w10, h10, min, 20, waterElevation);
            Process(result, grid10, w10, h10, min, 10, waterElevation);
        }

        private static bool[,] GenerateMap(TerrainPolygon polygon, Vector2 min, int w10, int h10)
        {
            var grid10 = new bool[w10, h10];
            using (var img = new Image<Rgba32>(w10, h10, Color.Transparent))
            {
                img.Mutate(d => PolygonDrawHelper.DrawPolygon(d, polygon, new SolidBrush(Color.White), l => l.Select(p => (PointF)((p.Vector - min) / 10f))));
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

        private void Process(List<TerrainBuilderObject> objects, bool[,] grid10, int w10, int h10, Vector2 min, int pondSize, float waterElevation)
        {
            var model = assets.GetPond(pondSize);
            var objSize = pondSize / 10;
            var hsize = pondSize / 2;
            for (int x = 0; x < w10; x += objSize)
            {
                for (int y = 0; y < h10; y += objSize)
                {
                    if (x + objSize <= w10 && y + objSize < h10 && Match(grid10, x, y, x + objSize, y + objSize))
                    {
                        Take(grid10, x, y, x + objSize, y + objSize);
                        var ax = (x * 10) + hsize;
                        var ay = (y * 10) + hsize;
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
