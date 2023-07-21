using System.Numerics;
using System.Runtime.Versioning;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.ManMade.Places;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Nature.Surfaces;
using GameRealisticMap.Nature.Watercourses;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using HugeImages.Storage;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Arma3
{
    public class Arma3DemoMapGenerator : Arma3MapGenerator
    {
        private readonly string name;

        public Arma3DemoMapGenerator(IArma3RegionAssets assets, ProjectDrive projectDrive, string name)
            : base(assets, projectDrive)
        {
            this.name = name;
        }

        protected override Task<IOsmDataSource> LoadOsmData(IProgressTask progress, Arma3MapConfig a3config)
        {
            return Task.FromResult<IOsmDataSource>(new NoneOsmDataSource());
        }

        protected override BuildContext CreateBuildContext(IProgressTask progress, Arma3MapConfig a3config, IOsmDataSource osmSource, IHugeImageStorage? hugeImageStorage = null)
        {
            var context = base.CreateBuildContext(progress, a3config, osmSource, hugeImageStorage);

            var grid = new ElevationGrid(a3config.TerrainArea.GridSize, a3config.TerrainArea.GridCellSize);
            grid.Fill(15f);
            context.SetData(new RawElevationData(grid));

            var places = new List<City>();

            CreateSampleSingle(context, places, polygon => new ForestData(polygon), new Vector2(0 * 512, 0));
            CreateSampleSingle(context, places, polygon => new ScrubData(polygon), new Vector2(1 * 512, 0));
            CreateSampleSingle(context, places, polygon => new RocksData(polygon), new Vector2(2 * 512, 0));
            CreateSampleSingle(context, places, polygon => new GrassData(polygon), new Vector2(3 * 512, 0));
            CreateSampleSingle(context, places, polygon => new MeadowsData(polygon), new Vector2(4 * 512, 0));
            CreateSampleSingle(context, places, polygon => new SandSurfacesData(polygon), new Vector2(5 * 512, 0));
            CreateSampleQuad(context, places, polygon => new FarmlandsData(polygon), new Vector2(0 * 512, 512));
            CreateSampleLakes(context, places, new Vector2(6 * 512, 0));
            CreateSampleWatercourses(context, places, new Vector2(7 * 512, 0));

            places.Add(new City(new TerrainPoint(a3config.TerrainArea.SizeInMeters / 2, a3config.TerrainArea.SizeInMeters / 2), new List<TerrainPolygon>() { a3config.TerrainArea.TerrainBounds }, name + " DEMO", CityTypeId.City, 1024, 10000));
            context.SetData(new CitiesData(places));

            CreateRoads(context,grid);

            return context;
        }

        private void CreateRoads(BuildContext context, ElevationGrid grid)
        {
            var roads = new List<Road>();
            int index = 0;
            foreach(var typeid in Enum.GetValues<RoadTypeId>())
            {
                CreateRoad(assets.RoadTypeLibrary.GetInfo(typeid), roads, grid, new Vector2(index * 512, 1024));
                index++;
            }
            context.SetData(new RoadsData(roads));
        }

        private void CreateRoad(IRoadTypeInfos typeid, List<Road> roads, ElevationGrid grid, Vector2 offset)
        {
            CreateBridgePattern(typeid, roads, grid, 25, 128, new TerrainPoint(128, 128) + offset);
            CreateBridgePattern(typeid, roads, grid, 25, 128, new TerrainPoint(128 + 256, 128) + offset, 20f);
            CreateBridgePattern(typeid, roads, grid, 75, 256, new TerrainPoint(256, 385) + offset);
            CreateBridgePattern(typeid, roads, grid, 75, 256, new TerrainPoint(256, 512 + 385) + offset, 20f);
        }

        private void CreateBridgePattern(IRoadTypeInfos typeid, List<Road> roads, ElevationGrid grid, int bridgeGap, int radius, TerrainPoint center, float centerElevation = 15f)
        {
            var centralRadius = 35;
            var adjust = 5;

            var min = center - new Vector2(radius);
            var max = center + new Vector2(radius);
            using (var mutate = grid.PrepareToMutate(min, max, 5, 25))
            {
                mutate.Image.Mutate(d =>
                {
                    d.Fill(new SolidBrush(mutate.ElevationToColor(10f)), new EllipsePolygon(mutate.ToPixel(center), MathF.Ceiling((centralRadius + bridgeGap - adjust) / grid.CellSize.X)));
                    d.Fill(new SolidBrush(mutate.ElevationToColor(centerElevation)), new EllipsePolygon(mutate.ToPixel(center), MathF.Floor((centralRadius + adjust) / grid.CellSize.X)));
                });
                mutate.Apply();
            }

            // N --> S
            roads.Add(new Road(WaySpecialSegment.Normal, new TerrainPath(center + new Vector2(0, radius), center + new Vector2(0, centralRadius + bridgeGap)), typeid));
            roads.Add(new Road(WaySpecialSegment.Bridge, new TerrainPath(center + new Vector2(0, centralRadius + bridgeGap), center + new Vector2(0, centralRadius)), typeid));
            roads.Add(new Road(WaySpecialSegment.Normal, new TerrainPath(center + new Vector2(0, centralRadius), center - new Vector2(0, centralRadius)), typeid));
            roads.Add(new Road(WaySpecialSegment.Bridge, new TerrainPath(center - new Vector2(0, centralRadius), center - new Vector2(0, centralRadius + bridgeGap)), typeid));
            roads.Add(new Road(WaySpecialSegment.Normal, new TerrainPath(center - new Vector2(0, centralRadius + bridgeGap), center - new Vector2(0, radius)), typeid));

            // W --> E
            roads.Add(new Road(WaySpecialSegment.Normal, new TerrainPath(center + new Vector2(radius, 0), center + new Vector2(centralRadius + bridgeGap, 0)), typeid));
            roads.Add(new Road(WaySpecialSegment.Bridge, new TerrainPath(center + new Vector2(centralRadius + bridgeGap, 0), center + new Vector2(centralRadius, 0)), typeid));
            roads.Add(new Road(WaySpecialSegment.Normal, new TerrainPath(center + new Vector2(centralRadius, 0), center - new Vector2(centralRadius, 0)), typeid));
            roads.Add(new Road(WaySpecialSegment.Bridge, new TerrainPath(center - new Vector2(centralRadius, 0), center - new Vector2(centralRadius + bridgeGap, 0)), typeid));
            roads.Add(new Road(WaySpecialSegment.Normal, new TerrainPath(center - new Vector2(centralRadius + bridgeGap, 0), center - new Vector2( radius, 0)), typeid));

        }

        private static void CreateSampleWatercourses(BuildContext context, List<City> places, Vector2 offset)
        {
            var river = new List<Watercourse>() { new Watercourse( new TerrainPath(
                new TerrainPoint(64, 64)+offset,
                new TerrainPoint(256, 64)+offset,
                new TerrainPoint(448, 448)+offset
                ), WatercourseTypeId.River) };

            context.SetData(new WatercoursesData(river, river.SelectMany(r => r.Polygons).ToList()));
            places.Add(new City(new TerrainPoint(256, 256) + offset, new List<TerrainPolygon>() { TerrainPolygon.FromRectangle(new TerrainPoint(28, 28) + offset, new TerrainPoint(484, 484) + offset) }, "Watercourses", CityTypeId.Village, 256, 0));
        }

        private static void CreateSampleLakes(BuildContext context, List<City> places, Vector2 offset)
        {
            context.SetData(new LakesData(new List<TerrainPolygon>() {
                TerrainPolygon.FromRectangle(new TerrainPoint(128, 128) + offset, new TerrainPoint(128 + 256, 256) + offset),
                TerrainPolygon.FromRectangle(new TerrainPoint(128, 320) + offset, new TerrainPoint(128 + 64, 384) + offset)
            }));
            places.Add(new City(new TerrainPoint(256, 256) + offset, new List<TerrainPolygon>() { TerrainPolygon.FromRectangle(new TerrainPoint(28, 28) + offset, new TerrainPoint(484, 484) + offset) }, "Lakes", CityTypeId.Village, 256, 0));
        }

        private void CreateSampleSingle<T>(BuildContext context, List<City> places, Func<List<TerrainPolygon>, T> value, Vector2 offset)
            where T : class
        {
            var polygon = TerrainPolygon.FromRectangle(new TerrainPoint(28, 28)+ offset, new TerrainPoint(484, 484) + offset);
            var polygons = new List<TerrainPolygon>() { polygon };
            var center = new TerrainPoint(256, 256) + offset;
            context.SetData<T>(value(polygons));
            places.Add(new City(center, polygons, typeof(T).Name.Replace("Data", ""), CityTypeId.Village, 256, 0));
        }

        private void CreateSampleQuad<T>(BuildContext context, List<City> places, Func<List<TerrainPolygon>, T> value, Vector2 offset)
            where T : class
        {
            var polygons = new List<TerrainPolygon>() {
                TerrainPolygon.FromRectangle(new TerrainPoint(28, 28) + offset, new TerrainPoint(228, 228) + offset),
                TerrainPolygon.FromRectangle(new TerrainPoint(28, 284) + offset, new TerrainPoint(228, 484) + offset),
                TerrainPolygon.FromRectangle(new TerrainPoint(284, 28) + offset, new TerrainPoint(484, 228) + offset),
                TerrainPolygon.FromRectangle(new TerrainPoint(284, 284) + offset, new TerrainPoint(484, 484) + offset)
            };
            var center = new TerrainPoint(256, 256) + offset;
            context.SetData<T>(value(polygons));
            places.Add(new City(center, polygons, typeof(T).Name.Replace("Data", ""), CityTypeId.Village, 256, 0));
        }
        private Arma3MapConfig CreateConfig()
        {
            return new Arma3MapConfig(new Arma3MapConfigJson()
            {
                WorldName = $"grm_demo_{name}",
                GridSize = 1024,
                GridCellSize = 5,
                FakeSatBlend = 1,
                SouthWest = "0, 0",
                Resolution = 1,
                TileSize = 512
            });
        }

        public async Task<Arma3MapConfig> GenerateWrp(IProgressTask progress)
        {
            var config = CreateConfig();
            await GenerateWrp(progress, config);
            return config;
        }

        [SupportedOSPlatform("windows")]
        public async Task<Arma3MapConfig> GenerateMod(IProgressTask progress)
        {
            var config = CreateConfig();
            await GenerateMod(progress, config);
            return config;
        }
    }
}
