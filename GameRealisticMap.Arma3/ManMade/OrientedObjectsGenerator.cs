using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Conditions;
using GameRealisticMap.ManMade.Objects;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.ManMade
{
    internal class OrientedObjectsGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IProgressSystem progress;
        private readonly IArma3RegionAssets assets;

        public OrientedObjectsGenerator(IProgressSystem progress, IArma3RegionAssets assets)
        {
            this.progress = progress;
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context)
        {
            var evaluator = context.GetData<ConditionEvaluator>();
            var objects = context.GetData<OrientedObjectData>().Objects;
            var result = new List<TerrainBuilderObject>();
            foreach(var obj in objects.ProgressStep(progress, "OrientedObjects"))
            {
                var candidates = assets.GetObjects(obj.TypeId);
                if (candidates.Count >  0)
                {
                    var pointContext = new PointConditionContext(evaluator, obj.Point, obj.Road);
                    var definition = candidates.GetRandom(obj.Point, pointContext);
                    if (definition != null)
                    {
                        result.AddRange(definition.Composition.ToTerrainBuilderObjects(new ModelPosition(obj.Point, obj.Angle)));
                    }
                }
            }

            var lamps = assets.GetObjects(ObjectTypeId.StreetLamp);
            if (lamps.Count > 0)
            {
                foreach (var obj in context.GetData<ProceduralStreetLampsData>().Objects.ProgressStep(progress, "ProceduralStreetLamps"))
                {
                    var pointContext = new PointConditionContext(evaluator, obj.Point, obj.Road);
                    var definition = lamps.GetRandom(obj.Point, pointContext);
                    if (definition != null)
                    {
                        result.AddRange(definition.Composition.ToTerrainBuilderObjects(new ModelPosition(obj.Point, obj.Angle)));
                    }
                }
            }
            return result;
        }
    }
}
