using System.Numerics;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Algorithms.Filling;
using GameRealisticMap.Algorithms.Following;
using GameRealisticMap.Algorithms.Rows;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.ManMade.Farmlands
{
    internal class OrchardGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IProgressSystem progress;
        private readonly IArma3RegionAssets assets;

        public OrchardGenerator(IProgressSystem progress, IArma3RegionAssets assets)
        {
            this.progress = progress;
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context)
        {
            var layer1 = new List<PlacedModel<Composition>>();
            var layer2 = new RadiusPlacedLayer<Composition>(new Vector2(config.SizeInMeters));
            var lib1 = assets.GetNaturalRows(Assets.Rows.NaturalRowType.OrchardRow);
            var lib2 = assets.GetBasicCollections(Assets.Filling.BasicCollectionId.SmallOrchard);
            if (lib1.Count > 0 || lib2.Count > 0)
            {
                var small = new List<TerrainPolygon>();
                var orchards = context.GetData<OrchardData>().Polygons;
                foreach (var orchard in orchards.ProgressStep(progress, "Orchards"))
                {
                    if (orchard.Area > 4000)
                    {
                        var rnd = RandomHelper.CreateRandom(orchard.Centroid);
                        FillAreaWithRows.Fill(rnd, lib1.GetRandom(rnd), layer1, orchard);
                    }
                    else
                    {
                        small.Add(orchard);
                    }
                }

                using (progress.CreateScope("SmallOrchards"))
                {
                    new FillAreaBasic<Composition>(progress, lib2).FillPolygons(layer2, small);
                }
            }
            return layer1.SelectMany(o => o.Model.ToTerrainBuilderObjects(o))
                .Concat(layer2.SelectMany(o => o.Model.ToTerrainBuilderObjects(o)));
        }
    }
}
