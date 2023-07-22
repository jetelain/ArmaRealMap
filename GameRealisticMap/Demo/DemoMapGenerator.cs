using System.Numerics;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.ManMade.Fences;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.ManMade.Places;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Nature.Surfaces;
using GameRealisticMap.Nature.Watercourses;
using GameRealisticMap.Preview;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace GameRealisticMap.Demo
{
    /// <summary>
    /// Generate a simple world to show case all features of a map generator.
    /// 
    /// This can be useful to debug a specific game engine generator, or to show case a specific configuration.
    /// </summary>
    public class DemoMapGenerator
    {
        private readonly IRoadTypeLibrary<IRoadTypeInfos> roadTypeLibrary;
        private readonly IDemoNaming demoNaming;

        public DemoMapGenerator(IRoadTypeLibrary<IRoadTypeInfos> roadTypeLibrary, IDemoNaming demoNaming)
        {
            this.roadTypeLibrary = roadTypeLibrary;
            this.demoNaming = demoNaming;
        }

        public void CreateInto(BuildContext context, string name)
        {
            CreateInto(context, name, new Dictionary<BuildingTypeId, List<Vector2>>()); // XXX: Create a typical building size database
        }

        public void CreateInto(BuildContext context, string name, Dictionary<BuildingTypeId,List<Vector2>> buildingTypicalSizes)
        {
            var grid = new ElevationGrid(context.Area.GridSize, context.Area.GridCellSize);
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
            context.SetData(new CategoryAreaData(
                new List<CategoryArea>()
                {
                   new CategoryArea(BuildingTypeId.Residential, CreateSquare512(places, demoNaming.GetSurfaceName(BuildingTypeId.Residential), new Vector2(1 * 512, 512))),
                   new CategoryArea(BuildingTypeId.Industrial, CreateSquare512(places, demoNaming.GetSurfaceName(BuildingTypeId.Industrial), new Vector2(2 * 512, 512)))
               }));


            CreateSampleFences(context, places);

            var roads = CreateSampleRoads(grid, places);

            CreateSampleOriented(context, roads, places);

            places.Add(new City(new TerrainPoint(context.Area.SizeInMeters / 2, context.Area.SizeInMeters / 2), new List<TerrainPolygon>() { context.Area.TerrainBounds }, name + " DEMO", CityTypeId.City, 1024, 10000));

            CreateBuildings(context, places, roads, context.Area, buildingTypicalSizes);

            context.SetData(new RoadsData(roads));
            context.SetData(new CitiesData(places));

#if DEBUG
            PreviewRender.RenderHtml(context, "demo.html").Wait();
#endif
        }

        private void CreateBuildings(BuildContext context, List<City> places, List<Road> roads, ITerrainArea area, Dictionary<BuildingTypeId, List<Vector2>> buildingTypicalSizes)
        {
            var buildings = new List<Building>();

            // 3072 + 128

            var centerY = 2048 + 512f;
            var x = 0f;

            var secondary = roadTypeLibrary.GetInfo(RoadTypeId.TwoLanesSecondaryRoad);
            var tertiary = roadTypeLibrary.GetInfo(RoadTypeId.TwoLanesConcreteRoad);

            var mainRoad = new Road(WaySpecialSegment.Normal, new TerrainPath(new(0, centerY), new(area.SizeInMeters, centerY)), secondary);
            roads.Add(mainRoad);

            foreach (var pair in buildingTypicalSizes)
            {
                var id = pair.Key;
                var hasLabel = false;

                foreach (var size in pair.Value)
                {
                    var patternSize = size.X * 2 + tertiary.ClearWidth + 64;
                    if (x + patternSize > area.SizeInMeters)
                    {
                        x = 0;
                        centerY += 512f;
                        mainRoad = new Road(WaySpecialSegment.Normal, new TerrainPath(new(0, centerY), new(area.SizeInMeters, centerY)), secondary);
                        roads.Add(mainRoad);
                        hasLabel = false;
                    }

                    if (!hasLabel)
                    {
                        places.Add(new City(new TerrainPoint(x + 4, centerY), new List<TerrainPolygon>(), demoNaming.GetBuildingName(id), CityTypeId.Village, 1, 0));
                        hasLabel = true;
                    }

                    var centerX = x + (patternSize / 2);

                    var perroad = new Road(WaySpecialSegment.Normal, new TerrainPath(new(centerX, centerY - 256), new(centerX, centerY + 256)), tertiary);
                    roads.Add(perroad);

                    var shiftX = size.X / 2 + tertiary.ClearWidth / 2 + 8;
                    var shiftY = size.Y / 2 + secondary.ClearWidth / 2 + 1;
                    AddRoad(buildings, mainRoad, id, perroad, new BoundingBox(new TerrainPoint(centerX - shiftX, centerY - shiftY), size.X, size.Y, 0));
                    AddRoad(buildings, mainRoad, id, perroad, new BoundingBox(new TerrainPoint(centerX + shiftX, centerY - shiftY), size.X, size.Y, 0));
                    AddRoad(buildings, mainRoad, id, perroad, new BoundingBox(new TerrainPoint(centerX - shiftX, centerY + shiftY), size.X, size.Y, 0));
                    AddRoad(buildings, mainRoad, id, perroad, new BoundingBox(new TerrainPoint(centerX + shiftX, centerY + shiftY), size.X, size.Y, 0));

                    shiftX = size.Y / 2 + tertiary.ClearWidth / 2 + 1;
                    shiftY = 128;
                    AddRoad(buildings, mainRoad, id, perroad, new BoundingBox(new TerrainPoint(centerX - shiftX, centerY - shiftY), size.Y, size.X, 0));
                    AddRoad(buildings, mainRoad, id, perroad, new BoundingBox(new TerrainPoint(centerX + shiftX, centerY - shiftY), size.Y, size.X, 0));
                    AddRoad(buildings, mainRoad, id, perroad, new BoundingBox(new TerrainPoint(centerX - shiftX, centerY + shiftY), size.Y, size.X, 0));
                    AddRoad(buildings, mainRoad, id, perroad, new BoundingBox(new TerrainPoint(centerX + shiftX, centerY + shiftY), size.Y, size.X, 0));

                    x += patternSize;
                }
                
            }


            context.SetData(new BuildingsData(buildings));
        }

        private static void AddRoad(List<Building> buildings, Road mainRoad, BuildingTypeId id, Road perroad, BoundingBox box)
        {
            var closestRoads = BoxSideHelper.GetClosestList(box, new[] { perroad, mainRoad }, 20, r => r.Path, r => r.Factor).ToList();
            buildings.Add(new Building(box, id, new List<TerrainPolygon>() { box.Polygon }, closestRoads.FirstOrDefault()));
        }

        private void CreateSampleOriented(BuildContext context, List<Road> roads, List<City> places)
        {
            var offset = new Vector2(4 * 512, 512);
            var suboffset = new Vector2(0, 0);

            var objects = new List<OrientedObject>();
            foreach (var obj in Enum.GetValues<ObjectTypeId>())
            {
                CreateSampleOriented(objects, roads, places, obj, offset + suboffset);
                suboffset.X += 102;
                if (suboffset.X >= 410)
                {
                    suboffset.Y += 102;
                    suboffset.X = 0;
                }
            }
            context.SetData(new OrientedObjectData(objects));
        }


        private void CreateSampleOriented(List<OrientedObject> objects, List<Road> roads, List<City> places, ObjectTypeId id, Vector2 offset)
        {
            var type = roadTypeLibrary.GetInfo(RoadTypeId.TwoLanesConcreteRoad);
            var margin = (type.ClearWidth / 2) + 2;
            roads.Add(new Road(WaySpecialSegment.Normal, new TerrainPath(new TerrainPoint(0, 51) + offset, new TerrainPoint(102, 51) + offset), type));
            roads.Add(new Road(WaySpecialSegment.Normal, new TerrainPath(new TerrainPoint(51, 0) + offset, new TerrainPoint(51, 102) + offset), type));
            places.Add(new City(new TerrainPoint(51, 51) + offset, new List<TerrainPolygon>(), demoNaming.GetObjectName(id), CityTypeId.Village, 51, 0));
            AddSampleObject(objects, roads, id, new TerrainPoint(51 - margin, 51 - margin) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(51 - margin, 51 + margin) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(51 + margin, 51 - margin) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(51 + margin, 51 + margin) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(17, 51 - margin) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(17, 51 + margin) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(34, 51 - margin) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(34, 51 + margin) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(68, 51 - margin) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(68, 51 + margin) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(85, 51 - margin) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(85, 51 + margin) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(51 - margin, 17) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(51 + margin, 17) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(51 - margin, 34) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(51 + margin, 34) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(51 - margin, 68) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(51 + margin, 68) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(51 - margin, 85) + offset);
            AddSampleObject(objects, roads, id, new TerrainPoint(51 + margin, 85) + offset);
        }

        private static void AddSampleObject(List<OrientedObject> objects, List<Road> roads, ObjectTypeId id, TerrainPoint p)
        {
            objects.Add(new OrientedObject(p, GeometryHelper.GetFacing(p, roads.Select(r => r.Path), 50) ?? 0, id));
        }

        private void CreateSampleFences(BuildContext context, List<City> places)
        {
            var fences = new List<Fence>();
            var offset = new Vector2(3 * 512, 512);
            CreateSampleFence(places, fences, offset, FenceTypeId.Fence);
            CreateSampleFence(places, fences, offset + new Vector2(120, 0), FenceTypeId.Hedge);
            CreateSampleFence(places, fences, offset + new Vector2(240, 0), FenceTypeId.Wall);
            context.SetData(new FencesData(fences));
        }

        private void CreateSampleFence(List<City> places, List<Fence> fences, Vector2 offset, FenceTypeId id)
        {
            fences.Add(new Fence(TerrainPath.FromRectangle(new TerrainPoint(8, 8) + offset, new TerrainPoint(18, 18) + offset), id));
            fences.Add(new Fence(TerrainPath.FromRectangle(new TerrainPoint(8, 22) + offset, new TerrainPoint(28, 42) + offset), id));
            fences.Add(new Fence(TerrainPath.FromRectangle(new TerrainPoint(8, 46) + offset, new TerrainPoint(48, 86) + offset), id));

            fences.Add(new Fence(TerrainPath.FromCircle(new TerrainPoint(78, 13) + offset, 5), id));
            fences.Add(new Fence(TerrainPath.FromCircle(new TerrainPoint(78, 32) + offset, 10), id));
            fences.Add(new Fence(TerrainPath.FromCircle(new TerrainPoint(78, 66) + offset, 20), id));

            fences.Add(new Fence(TerrainPath.FromCircle(new TerrainPoint(52, 150) + offset, 50), id));

            places.Add(new City(new TerrainPoint(52, 45) + offset, new List<TerrainPolygon>(), demoNaming.GetFenceName(id), CityTypeId.Village, 52, 0));
        }

        private List<Road> CreateSampleRoads(ElevationGrid grid, List<City> places)
        {
            var roads = new List<Road>();
            int index = 0;
            foreach (var typeid in Enum.GetValues<RoadTypeId>())
            {
                CreateRoad(roadTypeLibrary.GetInfo(typeid), roads, grid, new Vector2(index * 512, 1024), places);
                index++;
            }
            var y = 1024f + 518f;
            foreach (var typeid in Enum.GetValues<RoadTypeId>())
            {
                var type = roadTypeLibrary.GetInfo(typeid);

                roads.Add(new Road(WaySpecialSegment.Normal, new TerrainPath(new(0, y), new(5120, y)), type));

                y += type.ClearWidth + 10;
            }

            return roads;
        }

        private void CreateRoad(IRoadTypeInfos typeid, List<Road> roads, ElevationGrid grid, Vector2 offset, List<City> places)
        {
            places.Add(new City(new TerrainPoint(256, 128) + offset, new List<TerrainPolygon>(), demoNaming.GetRoadName(typeid.Id), CityTypeId.Village, 256, 0));
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
            roads.Add(new Road(WaySpecialSegment.Normal, new TerrainPath(center - new Vector2(centralRadius + bridgeGap, 0), center - new Vector2(radius, 0)), typeid));

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
            context.SetData<T>(value(CreateSquare512(places, demoNaming.GetSurfaceName(typeof(T)), offset)));
        }

        private List<TerrainPolygon> CreateSquare512(List<City> places, string name, Vector2 offset)
        {
            var polygons = new List<TerrainPolygon>() { TerrainPolygon.FromRectangle(new TerrainPoint(28, 28) + offset, new TerrainPoint(484, 484) + offset) };
            var center = new TerrainPoint(256, 256) + offset;
            places.Add(new City(center, polygons, name, CityTypeId.Village, 256, 0));
            return polygons;
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
            places.Add(new City(center, polygons, demoNaming.GetSurfaceName(typeof(T)), CityTypeId.Village, 256, 0));
        }
    }
}
