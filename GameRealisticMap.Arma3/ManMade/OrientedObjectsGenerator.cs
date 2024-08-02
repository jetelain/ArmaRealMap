using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Conditions;
using GameRealisticMap.ManMade.Objects;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.ManMade
{
    internal class OrientedObjectsGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IArma3RegionAssets assets;

        public OrientedObjectsGenerator(IArma3RegionAssets assets)
        {
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context, IProgressScope scope)
        {
            var evaluator = context.GetData<ConditionEvaluator>();
            var objects = context.GetData<OrientedObjectData>().Objects;
            var result = new List<TerrainBuilderObject>();
            foreach(var obj in objects.WithProgress(scope, "OrientedObjects"))
            {
                var candidates = assets.GetObjects(obj.TypeId);
                if (candidates.Count >  0)
                {
                    var definition = candidates.GetRandom(obj.Point, evaluator.GetPointContext(obj.Point, obj.Road));
                    if (definition != null)
                    {
                        result.AddRange(definition.Composition.ToTerrainBuilderObjects(new ModelPosition(obj.Point, obj.Angle)));
                    }
                }
            }

            var lamps = assets.GetObjects(ObjectTypeId.StreetLamp);
            if (lamps.Count > 0)
            {
                foreach (var obj in context.GetData<ProceduralStreetLampsData>().Objects.WithProgress(scope, "ProceduralStreetLamps"))
                {
                    var definition = lamps.GetRandom(obj.Point, evaluator.GetPointContext(obj.Point, obj.Road));
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
