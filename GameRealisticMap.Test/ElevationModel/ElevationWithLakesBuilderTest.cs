using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Airports;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Railways;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Osm;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Test.ElevationModel
{
    public class ElevationWithLakesBuilderTest
    {
        [Fact]
        public void Build_Lake()
        {
            var context = new BuildContextMock(TerrainAreaUTM.CreateFromCenter(new CoordinateSharp.Coordinate(0, 0), 2.5f, 128), new NoneOsmDataSource());
            var grid = new ElevationGrid(context.Area.GridSize, context.Area.GridCellSize);
            grid.Fill(5);
            context.SetData(new RawElevationData(grid));
            context.SetData(new BuildingsData(new List<Building>()));
            context.SetData(new RoadsData(new List<Road>()));
            context.SetData(new RailwaysData(new List<Railway>()));
            context.SetData(new LakesData(new List<TerrainPolygon>() {
                TerrainPolygon.FromRectangle(new TerrainPoint(10, 100), new TerrainPoint(50, 200))
            }));
            context.SetData(new AerowaysData(new List<AirportAeroways>(), new List<Aeroway>()));

            var builder = new ElevationWithLakesBuilder();
            var result = builder.Build(context, new NoProgress());
            var lake = Assert.Single(result.Lakes);
            Assert.Equal(5, lake.BorderElevation);
            Assert.Equal(4.8, lake.WaterElevation, 2);
            Assert.Equal(10.3, lake.TerrainPolygon.MinPoint.X, 1);
            Assert.Equal(100.4, lake.TerrainPolygon.MinPoint.Y, 1);
            Assert.Equal(47.2, lake.TerrainPolygon.MaxPoint.X, 1);
            Assert.Equal(197.2, lake.TerrainPolygon.MaxPoint.Y, 1);
            Assert.Empty(lake.TerrainPolygon.Holes);
            Assert.InRange(lake.TerrainPolygon.Area, 3350, 3400);
        }

        [Fact]
        public void Build_Lake_Island()
        {
            var context = new BuildContextMock(TerrainAreaUTM.CreateFromCenter(new CoordinateSharp.Coordinate(0, 0), 2.5f, 128), new NoneOsmDataSource());
            var grid = new ElevationGrid(context.Area.GridSize, context.Area.GridCellSize);
            grid.Fill(5);
            context.SetData(new RawElevationData(grid));
            context.SetData(new BuildingsData(new List<Building>()));
            context.SetData(new RoadsData(new List<Road>()));
            context.SetData(new RailwaysData(new List<Railway>()));
            context.SetData(new LakesData(
                TerrainPolygon.FromRectangle(new TerrainPoint(10, 100), new TerrainPoint(110, 200)).Substract(TerrainPolygon.FromRectangle(new TerrainPoint(50, 140), new TerrainPoint(70, 160))).ToList()
            ));
            context.SetData(new AerowaysData(new List<AirportAeroways>(), new List<Aeroway>()));

            var builder = new ElevationWithLakesBuilder();
            var result = builder.Build(context, new NoProgress());
            var lake = Assert.Single(result.Lakes);
            Assert.Equal(5, lake.BorderElevation);
            Assert.Equal(4.8, lake.WaterElevation, 2);
            Assert.Equal(10.3, lake.TerrainPolygon.MinPoint.X, 1);
            Assert.Equal(100.4, lake.TerrainPolygon.MinPoint.Y, 1);
            Assert.Equal(107.2, lake.TerrainPolygon.MaxPoint.X, 1);
            Assert.Equal(197.2, lake.TerrainPolygon.MaxPoint.Y, 1);
            Assert.InRange(lake.TerrainPolygon.Area, 8670, 8680);

            var island = new TerrainPolygon(Assert.Single(lake.TerrainPolygon.Holes));
            Assert.Equal(47.2, island.MinPoint.X, 1);
            Assert.Equal(137.4, island.MinPoint.Y, 1);
            Assert.Equal(70.3, island.MaxPoint.X, 1);
            Assert.Equal(160.3, island.MaxPoint.Y, 1);
            Assert.InRange(island.Area, 490, 510);
        }
    }
}
