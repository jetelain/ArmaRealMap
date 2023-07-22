using System.Numerics;
using System.Runtime.Versioning;
using BIS.WRP;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Demo;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Places;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Osm;
using GameRealisticMap.Reporting;
using HugeImages.Storage;

namespace GameRealisticMap.Arma3
{
    public class Arma3DemoMapGenerator : Arma3MapGenerator
    {
        private static readonly string arrowModel = "A3\\Misc_F\\Helpers\\Sign_Arrow_Large_F.p3d";
        private static readonly Vector3 arrowBoundingCenter = new Vector3(0, 0.75f, 0);

        private readonly string name;
        private readonly IDemoNaming demoNaming;

        public Arma3DemoMapGenerator(IArma3RegionAssets assets, ProjectDrive projectDrive, string name, IDemoNaming? demoNaming = null)
            : base(assets, projectDrive)
        {
            this.name = name;
            this.demoNaming = demoNaming ?? new DefaultDemoNaming();
        }

        protected override Task<IOsmDataSource> LoadOsmData(IProgressTask progress, Arma3MapConfig a3config)
        {
            return Task.FromResult<IOsmDataSource>(new NoneOsmDataSource());
        }

        protected override BuildContext CreateBuildContext(IProgressTask progress, Arma3MapConfig a3config, IOsmDataSource osmSource, IHugeImageStorage? hugeImageStorage = null)
        {
            var context = base.CreateBuildContext(progress, a3config, osmSource, hugeImageStorage);

            var buildingSizes = Enum.GetValues<BuildingTypeId>().ToDictionary(id => id, id => assets.GetBuildings(id).Select(b => b.Size).ToList());

            new DemoMapGenerator(assets.RoadTypeLibrary, demoNaming).CreateInto(context, name, buildingSizes);

            var grid = context.GetData<ElevationData>().Elevation;
            var places = context.GetData<CitiesData>().Cities;

            var objects = new List<EditableWrpObject>();

            BridgeAdjust(grid, places, objects);

            BuildingAdjust(grid, places, objects);

            context.SetData(objects);

            return context;
        }

        private void BridgeAdjust(ElevationGrid grid, List<City> places, List<EditableWrpObject> objects)
        {
            var y = 1024f + 32f;
            var x = 256f;
            foreach (var id in Enum.GetValues<RoadTypeId>())
            {
                var roadType = assets.RoadTypeLibrary.GetInfo(id);
                var bridge = assets.GetBridge(id);
                if (bridge != null)
                {
                    var sx = x - roadType.ClearWidth * 4;

                    AddBridgeSegment(grid, objects, roadType, new TerrainPoint(sx + roadType.ClearWidth, y), bridge.Start);
                    sx += roadType.ClearWidth * 2;

                    AddBridgeSegment(grid, objects, roadType, new TerrainPoint(sx + roadType.ClearWidth, y), bridge.Middle);
                    sx += roadType.ClearWidth * 2;

                    AddBridgeSegment(grid, objects, roadType, new TerrainPoint(sx + roadType.ClearWidth, y), bridge.End);
                    sx += roadType.ClearWidth * 2;

                    AddBridgeSegment(grid, objects, roadType, new TerrainPoint(sx + roadType.ClearWidth, y), bridge.Single);
                }

                x += 512;
            }
        }

        private void BuildingAdjust(ElevationGrid grid, List<City> places, List<EditableWrpObject> objects)
        {
            var y = 4352f;
            var x = 5f;
            foreach (var id in Enum.GetValues<BuildingTypeId>())
            {
                var hasLabel = false;

                foreach (var compo in assets.GetBuildings(id))
                {
                    if (!hasLabel)
                    {
                        places.Add(new City(new TerrainPoint(x + 1, y), new List<TerrainPolygon>(), demoNaming.GetBuildingName(id), CityTypeId.Village, 1, 0));
                        hasLabel = true;
                    }

                    var center = new TerrainPoint(x + (compo.Size.X / 2), y);
                    var pos = new ModelPosition(center, 0, 0, 1);
                    objects.AddRange(compo.Composition.ToTerrainBuilderObjects(pos).Select(o => o.ToWrpObject(grid)));
                    AddBuildingAdjustArrows(objects, center + new Vector2(+compo.Size.X / 2, +compo.Size.Y / 2));
                    AddBuildingAdjustArrows(objects, center + new Vector2(-compo.Size.X / 2, +compo.Size.Y / 2));
                    AddBuildingAdjustArrows(objects, center + new Vector2(+compo.Size.X / 2, -compo.Size.Y / 2));
                    AddBuildingAdjustArrows(objects, center + new Vector2(-compo.Size.X / 2, -compo.Size.Y / 2));
                    if (compo.Size.Y > 5)
                    {
                        AddBuildingAdjustArrows(objects, center + new Vector2(-compo.Size.X / 2, 0));
                        AddBuildingAdjustArrows(objects, center + new Vector2(+compo.Size.X / 2, 0));
                    }
                    if (compo.Size.X > 5)
                    {
                        AddBuildingAdjustArrows(objects, center + new Vector2(0, -compo.Size.Y / 2));
                        AddBuildingAdjustArrows(objects, center + new Vector2(0, +compo.Size.Y / 2));
                    }
                    x += compo.Size.X + 10;
                }
            }

        }


        protected override IEnumerable<EditableWrpObject> GetObjects(IProgressTask progress, IArma3MapConfig config, IContext context, Arma3LayerGeneratorCatalog generators, ElevationGrid grid)
        {
            return base.GetObjects(progress, config, context, generators, grid)
                .Concat(context.GetData<List<EditableWrpObject>>());
        }


        private void AddBridgeSegment(ElevationGrid grid, List<EditableWrpObject> objects, Arma3RoadTypeInfos roadType, TerrainPoint center, StraightSegmentDefinition segment)
        {
            var pos = new ModelPosition(center, 0, 5, 1);
            objects.AddRange(segment.Model.ToTerrainBuilderObjects(pos).Select(o => o.ToWrpObject(grid)));
            AddBridgeAdjustArrows(objects,center + new Vector2( roadType.Width / 2, segment.Size / 2 ), 20f);
            AddBridgeAdjustArrows(objects,center + new Vector2(-roadType.Width / 2, segment.Size / 2 ), 20f);
            AddBridgeAdjustArrows(objects,center + new Vector2( roadType.Width / 2, -segment.Size / 2), 20f);
            AddBridgeAdjustArrows(objects,center + new Vector2(-roadType.Width / 2, -segment.Size / 2), 20f);
            AddBridgeAdjustArrows(objects,center + new Vector2(roadType.Width / 2, 0), 20f);
            AddBridgeAdjustArrows(objects,center + new Vector2(-roadType.Width / 2, 0), 20f);
        }

        private EditableWrpObject CreateArrow(TerrainPoint terrainPoint, float elevation)
        {
            return new EditableWrpObject() { 
                Model = arrowModel, 
                Transform = new BIS.Core.Math.Matrix4P(Matrix4x4.CreateTranslation(new Vector3(terrainPoint.X, elevation, terrainPoint.Y) + arrowBoundingCenter)) 
            };
        }
        private EditableWrpObject CreateReverseArrow(TerrainPoint terrainPoint, float elevation)
        {
            return new EditableWrpObject()
            {
                Model = arrowModel,
                Transform = new BIS.Core.Math.Matrix4P(Matrix4x4.CreateTranslation(new Vector3(terrainPoint.X, elevation, terrainPoint.Y) + arrowBoundingCenter) * Matrix4x4.CreateRotationZ(MathF.PI, new Vector3(terrainPoint.X, elevation, terrainPoint.Y)))
            };
        }

        private void AddBridgeAdjustArrows(List<EditableWrpObject> objects, TerrainPoint terrainPoint, float elevation)
        {
            objects.Add(CreateArrow(terrainPoint, elevation));
            objects.Add(CreateReverseArrow(terrainPoint, elevation));
            objects.Add(CreateArrow(terrainPoint, 15f)); // Ground
        }

        private void AddBuildingAdjustArrows(List<EditableWrpObject> objects, TerrainPoint terrainPoint)
        {
            objects.Add(CreateArrow(terrainPoint, 19.5f));
            objects.Add(CreateArrow(terrainPoint, 18f));
            objects.Add(CreateArrow(terrainPoint, 16.5f));
            objects.Add(CreateArrow(terrainPoint, 15f)); // Ground
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
