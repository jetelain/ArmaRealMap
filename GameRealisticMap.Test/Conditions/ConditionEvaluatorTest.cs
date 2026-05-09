using GameRealisticMap.Conditions;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Places;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Nature.Ocean;
using GameRealisticMap.Osm;

namespace GameRealisticMap.Test.Conditions
{
    public class ConditionEvaluatorTest
    {
        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static readonly TerrainAreaMock Area = new TerrainAreaMock(1000f, 10f);

        private static readonly IOsmDataSource EmptyOsm =
            OsmDataSource.CreateFromInlineXml("<osm version=\"0.6\"></osm>");

        private static BuildContextMock CreateContext(
            CategoryAreaData? areas = null,
            CitiesData? cities = null,
            ElevationData? elevation = null,
            OceanData? ocean = null,
            RoadsData? roads = null)
        {
            var ctx = new BuildContextMock(Area, EmptyOsm);
            ctx.SetData(areas ?? new CategoryAreaData(new List<CategoryArea>()));
            ctx.SetData(cities ?? new CitiesData(new List<City>()));
            ctx.SetData(elevation ?? FlatElevation(100f));
            ctx.SetData(ocean ?? new OceanData(new List<TerrainPolygon>(), new List<TerrainPolygon>(), false));
            ctx.SetData(roads ?? new RoadsData(new List<Road>()));
            return ctx;
        }

        private static ElevationData FlatElevation(float value)
        {
            var grid = new ElevationGrid(Area.GridSize, Area.GridCellSize);
            grid.Fill(value);
            return new ElevationData(grid);
        }

        private static Road MakeRoad(TerrainPoint from, TerrainPoint to, RoadTypeId type = RoadTypeId.TwoLanesConcreteRoad)
        {
            var infos = new DefaultRoadTypeInfos(type, 6f, 8f);
            var path = new TerrainPath(new List<TerrainPoint> { from, to });
            return new Road(WaySpecialSegment.Normal, path, infos);
        }

        private static TerrainPolygon Square(float x, float y, float size)
        {
            return new TerrainPolygon(new List<TerrainPoint>
            {
                new(x, y), new(x + size, y), new(x + size, y + size), new(x, y + size), new(x, y)
            });
        }

        // -----------------------------------------------------------------------
        // GetPointContext – elevation
        // -----------------------------------------------------------------------

        [Fact]
        public void GetPointContext_Elevation_ReturnsCorrectValue()
        {
            var ctx = CreateContext(elevation: FlatElevation(42f));
            var evaluator = new ConditionEvaluator(ctx);

            var pctx = evaluator.GetPointContext(new TerrainPoint(500, 500));

            Assert.Equal(42f, pctx.Elevation, 1f);
        }

        // -----------------------------------------------------------------------
        // GetPointContext – area flags
        // -----------------------------------------------------------------------

        [Fact]
        public void GetPointContext_InsideResidentialArea_IsResidentialTrue()
        {
            var areas = new CategoryAreaData(new List<CategoryArea>
            {
                new CategoryArea(BuildingTypeId.Residential, new List<TerrainPolygon> { Square(400, 400, 200) })
            });
            var evaluator = new ConditionEvaluator(CreateContext(areas: areas));

            var pctx = evaluator.GetPointContext(new TerrainPoint(500, 500));

            Assert.True(pctx.IsResidential);
            Assert.False(pctx.IsCommercial);
            Assert.False(pctx.IsIndustrial);
            Assert.True(pctx.IsUrban);
        }

        [Fact]
        public void GetPointContext_OutsideResidentialArea_IsResidentialFalse()
        {
            var areas = new CategoryAreaData(new List<CategoryArea>
            {
                new CategoryArea(BuildingTypeId.Residential, new List<TerrainPolygon> { Square(400, 400, 200) })
            });
            var evaluator = new ConditionEvaluator(CreateContext(areas: areas));

            var pctx = evaluator.GetPointContext(new TerrainPoint(100, 100));

            Assert.False(pctx.IsResidential);
            Assert.False(pctx.IsUrban);
        }

        [Theory]
        [InlineData(BuildingTypeId.Military)]
        [InlineData(BuildingTypeId.Commercial)]
        [InlineData(BuildingTypeId.Industrial)]
        [InlineData(BuildingTypeId.Retail)]
        [InlineData(BuildingTypeId.Agricultural)]
        public void GetPointContext_InsideOtherArea_CorrectFlagTrue(BuildingTypeId type)
        {
            var areas = new CategoryAreaData(new List<CategoryArea>
            {
                new CategoryArea(type, new List<TerrainPolygon> { Square(400, 400, 200) })
            });
            var evaluator = new ConditionEvaluator(CreateContext(areas: areas));
            var pctx = evaluator.GetPointContext(new TerrainPoint(500, 500));

            Assert.Equal(type == BuildingTypeId.Military, pctx.IsMilitary);
            Assert.Equal(type == BuildingTypeId.Commercial, pctx.IsCommercial);
            Assert.Equal(type == BuildingTypeId.Industrial, pctx.IsIndustrial);
            Assert.Equal(type == BuildingTypeId.Retail, pctx.IsRetail);
            Assert.Equal(type == BuildingTypeId.Agricultural, pctx.IsFarmyard);
        }

        // -----------------------------------------------------------------------
        // GetPointContext – ocean
        // -----------------------------------------------------------------------

        [Fact]
        public void GetPointContext_NoOcean_IsOceanFalse()
        {
            var evaluator = new ConditionEvaluator(CreateContext());
            Assert.False(evaluator.GetPointContext(new TerrainPoint(500, 500)).IsOcean);
        }

        [Fact]
        public void GetPointContext_NoOcean_DistanceToOceanIsMaxValue()
        {
            var evaluator = new ConditionEvaluator(CreateContext());
            Assert.Equal(float.MaxValue, evaluator.GetPointContext(new TerrainPoint(500, 500)).DistanceToOcean);
        }

        [Fact]
        public void GetPointContext_InsideOceanPolygon_IsOceanTrue()
        {
            var oceanPoly = Square(400, 400, 200);
            var ocean = new OceanData(new List<TerrainPolygon> { oceanPoly }, new List<TerrainPolygon>(), false);
            // Elevation must be ≤ 2.5 for the ocean polygon check to run
            var elevation = FlatElevation(0f);
            var evaluator = new ConditionEvaluator(CreateContext(ocean: ocean, elevation: elevation));

            Assert.True(evaluator.GetPointContext(new TerrainPoint(500, 500)).IsOcean);
        }

        [Fact]
        public void GetPointContext_VeryLowElevation_IsOceanTrue()
        {
            // Elevation < -3 → ocean, but only after the "no polygons" early-return is bypassed
            var oceanPoly = Square(0, 0, 999); // covers the whole terrain
            var ocean = new OceanData(new List<TerrainPolygon> { oceanPoly }, new List<TerrainPolygon>(), false);
            var elevation = FlatElevation(-5f);
            var evaluator = new ConditionEvaluator(CreateContext(ocean: ocean, elevation: elevation));

            Assert.True(evaluator.GetPointContext(new TerrainPoint(500, 500)).IsOcean);
        }

        // -----------------------------------------------------------------------
        // GetPointContext – roads
        // -----------------------------------------------------------------------

        [Fact]
        public void GetPointContext_NoRoads_DistanceToRoadIsMax()
        {
            var evaluator = new ConditionEvaluator(CreateContext());
            var pctx = evaluator.GetPointContext(new TerrainPoint(500, 500));

            Assert.Equal(ConditionEvaluator.MaxRoadDistance, pctx.DistanceToRoad);
            Assert.False(pctx.IsRoadSimple);
            Assert.False(pctx.IsRoadPrimary);
            Assert.False(pctx.IsRoadMotorway);
            Assert.False(pctx.IsRoadPath);
        }

        [Fact]
        public void GetPointContext_NearbyConcreteRoad_IsRoadSimpleTrue()
        {
            var road = MakeRoad(new TerrainPoint(495, 500), new TerrainPoint(505, 500), RoadTypeId.TwoLanesConcreteRoad);
            var roads = new RoadsData(new List<Road> { road });
            var evaluator = new ConditionEvaluator(CreateContext(roads: roads));

            var pctx = evaluator.GetPointContext(new TerrainPoint(500, 510));

            Assert.True(pctx.IsRoadSimple);
            Assert.False(pctx.IsRoadPrimary);
            Assert.True(pctx.DistanceToRoad < ConditionEvaluator.MaxRoadDistance);
        }

        [Fact]
        public void GetPointContext_NearbyPrimaryRoad_IsRoadPrimaryTrue()
        {
            var road = MakeRoad(new TerrainPoint(495, 500), new TerrainPoint(505, 500), RoadTypeId.TwoLanesPrimaryRoad);
            var roads = new RoadsData(new List<Road> { road });
            var evaluator = new ConditionEvaluator(CreateContext(roads: roads));

            var pctx = evaluator.GetPointContext(new TerrainPoint(500, 510));

            Assert.True(pctx.IsRoadPrimary);
        }

        [Fact]
        public void GetPointContext_NearbySecondaryRoad_IsRoadSecondaryTrue()
        {
            var road = MakeRoad(new TerrainPoint(495, 500), new TerrainPoint(505, 500), RoadTypeId.TwoLanesSecondaryRoad);
            var roads = new RoadsData(new List<Road> { road });
            var evaluator = new ConditionEvaluator(CreateContext(roads: roads));

            var pctx = evaluator.GetPointContext(new TerrainPoint(500, 510));

            Assert.True(pctx.IsRoadSecondary);
        }

        [Fact]
        public void GetPointContext_NearbyMotorway_IsRoadMotorwayTrue()
        {
            var road = MakeRoad(new TerrainPoint(495, 500), new TerrainPoint(505, 500), RoadTypeId.TwoLanesMotorway);
            var roads = new RoadsData(new List<Road> { road });
            var evaluator = new ConditionEvaluator(CreateContext(roads: roads));

            var pctx = evaluator.GetPointContext(new TerrainPoint(500, 510));

            Assert.True(pctx.IsRoadMotorway);
        }

        [Fact]
        public void GetPointContext_NearbyDirtPath_IsRoadPathTrue()
        {
            var road = MakeRoad(new TerrainPoint(495, 500), new TerrainPoint(505, 500), RoadTypeId.SingleLaneDirtPath);
            var roads = new RoadsData(new List<Road> { road });
            var evaluator = new ConditionEvaluator(CreateContext(roads: roads));

            var pctx = evaluator.GetPointContext(new TerrainPoint(500, 510));

            Assert.True(pctx.IsRoadPath);
        }

        // -----------------------------------------------------------------------
        // GetPathContext
        // -----------------------------------------------------------------------

        [Fact]
        public void GetPathContext_ReturnsCorrectLength()
        {
            var evaluator = new ConditionEvaluator(CreateContext());
            var path = new TerrainPath(new List<TerrainPoint> { new(100, 100), new(200, 100) });

            var pctx = evaluator.GetPathContext(path);

            Assert.Equal(path.Length, pctx.Length, 1f);
        }

        [Fact]
        public void GetPathContext_FlatElevation_ReturnsCorrectMinMaxAvg()
        {
            var evaluator = new ConditionEvaluator(CreateContext(elevation: FlatElevation(55f)));
            var path = new TerrainPath(new List<TerrainPoint> { new(100, 100), new(200, 100) });

            var pctx = evaluator.GetPathContext(path);

            Assert.Equal(55f, pctx.MinElevation, 1f);
            Assert.Equal(55f, pctx.MaxElevation, 1f);
            Assert.Equal(55f, pctx.AvgElevation, 1f);
        }

        [Fact]
        public void GetPathContext_InsideResidentialArea_IsResidentialTrue()
        {
            var areas = new CategoryAreaData(new List<CategoryArea>
            {
                new CategoryArea(BuildingTypeId.Residential, new List<TerrainPolygon> { Square(50, 50, 500) })
            });
            var evaluator = new ConditionEvaluator(CreateContext(areas: areas));
            var path = new TerrainPath(new List<TerrainPoint> { new(100, 100), new(200, 100) });

            Assert.True(evaluator.GetPathContext(path).IsResidential);
        }

        [Fact]
        public void GetPathContext_OutsideArea_IsResidentialFalse()
        {
            var evaluator = new ConditionEvaluator(CreateContext());
            var path = new TerrainPath(new List<TerrainPoint> { new(100, 100), new(200, 100) });

            Assert.False(evaluator.GetPathContext(path).IsResidential);
        }

        // -----------------------------------------------------------------------
        // GetPolygonContext
        // -----------------------------------------------------------------------

        [Fact]
        public void GetPolygonContext_ReturnsCorrectArea()
        {
            var evaluator = new ConditionEvaluator(CreateContext());
            var polygon = Square(100, 100, 100);

            var pctx = evaluator.GetPolygonContext(polygon);

            Assert.Equal((float)polygon.Area, pctx.Area, 1f);
        }

        [Fact]
        public void GetPolygonContext_FlatElevation_ReturnsCorrectMinMaxAvg()
        {
            var evaluator = new ConditionEvaluator(CreateContext(elevation: FlatElevation(77f)));
            var polygon = Square(100, 100, 100);

            var pctx = evaluator.GetPolygonContext(polygon);

            Assert.Equal(77f, pctx.MinElevation, 1f);
            Assert.Equal(77f, pctx.MaxElevation, 1f);
            Assert.Equal(77f, pctx.AvgElevation, 1f);
        }

        [Fact]
        public void GetPolygonContext_InsideMilitaryArea_IsMilitaryTrue()
        {
            var areas = new CategoryAreaData(new List<CategoryArea>
            {
                new CategoryArea(BuildingTypeId.Military, new List<TerrainPolygon> { Square(0, 0, 999) })
            });
            var evaluator = new ConditionEvaluator(CreateContext(areas: areas));
            var polygon = Square(100, 100, 100);

            Assert.True(evaluator.GetPolygonContext(polygon).IsMilitary);
        }

        [Fact]
        public void GetPolygonContext_OutsideArea_IsMilitaryFalse()
        {
            var evaluator = new ConditionEvaluator(CreateContext());
            Assert.False(evaluator.GetPolygonContext(Square(100, 100, 100)).IsMilitary);
        }

        // -----------------------------------------------------------------------
        // IsArea (point)
        // -----------------------------------------------------------------------

        [Fact]
        public void IsArea_PointInsideArea_ReturnsTrue()
        {
            var areas = new CategoryAreaData(new List<CategoryArea>
            {
                new CategoryArea(BuildingTypeId.Industrial, new List<TerrainPolygon> { Square(400, 400, 200) })
            });
            var evaluator = new ConditionEvaluator(CreateContext(areas: areas));

            Assert.True(evaluator.IsArea(new TerrainPoint(500, 500), BuildingTypeId.Industrial));
        }

        [Fact]
        public void IsArea_PointOutsideArea_ReturnsFalse()
        {
            var areas = new CategoryAreaData(new List<CategoryArea>
            {
                new CategoryArea(BuildingTypeId.Industrial, new List<TerrainPolygon> { Square(400, 400, 200) })
            });
            var evaluator = new ConditionEvaluator(CreateContext(areas: areas));

            Assert.False(evaluator.IsArea(new TerrainPoint(100, 100), BuildingTypeId.Industrial));
        }

        // -----------------------------------------------------------------------
        // GetAreas (point)
        // -----------------------------------------------------------------------

        [Fact]
        public void GetAreas_PointInsideMultipleAreas_ReturnsAll()
        {
            var areas = new CategoryAreaData(new List<CategoryArea>
            {
                new CategoryArea(BuildingTypeId.Residential, new List<TerrainPolygon> { Square(400, 400, 200) }),
                new CategoryArea(BuildingTypeId.Commercial,  new List<TerrainPolygon> { Square(450, 450, 100) }),
            });
            var evaluator = new ConditionEvaluator(CreateContext(areas: areas));

            var result = evaluator.GetAreas(new TerrainPoint(500, 500)).ToList();

            Assert.Contains(BuildingTypeId.Residential, result);
            Assert.Contains(BuildingTypeId.Commercial, result);
        }

        // -----------------------------------------------------------------------
        // DistanceToOcean
        // -----------------------------------------------------------------------

        [Fact]
        public void DistanceToOcean_WithOceanPolygon_ReturnsPositiveDistance()
        {
            var oceanPoly = Square(800, 800, 150);
            var ocean = new OceanData(new List<TerrainPolygon> { oceanPoly }, new List<TerrainPolygon>(), false);
            var evaluator = new ConditionEvaluator(CreateContext(ocean: ocean));

            var dist = evaluator.DistanceToOcean(new TerrainPoint(100, 100));

            Assert.True(dist > 0);
            Assert.True(dist < float.MaxValue);
        }
    }
}
