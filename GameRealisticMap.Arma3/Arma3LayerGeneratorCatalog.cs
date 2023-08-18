using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.ManMade;
using GameRealisticMap.Arma3.ManMade.Farmlands;
using GameRealisticMap.Arma3.Nature.Forests;
using GameRealisticMap.Arma3.Nature.Lakes;
using GameRealisticMap.Arma3.Nature.RockAreas;
using GameRealisticMap.Arma3.Nature.Scrubs;
using GameRealisticMap.Arma3.Nature.Trees;
using GameRealisticMap.Arma3.Nature.Watercourses;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3
{
    public class Arma3LayerGeneratorCatalog
    {
        public Arma3LayerGeneratorCatalog(IProgressSystem progress, IArma3RegionAssets assets)
        {
            // All generators

            // ManMade
            Generators.Add(new BuildingGenerator(progress, assets));
            Generators.Add(new OrientedObjectsGenerator(progress, assets));
            Generators.Add(new BridgeGenerator(progress, assets));
            Generators.Add(new FenceGenerator(progress, assets));
            Generators.Add(new RailwayGenerator(progress, assets));
            Generators.Add(new VineyardsGenerator(progress, assets));
            Generators.Add(new OrchardGenerator(progress, assets));

            // Nature
            Generators.Add(new ForestEdgeGenerator(progress, assets));
            Generators.Add(new ForestGenerator(progress, assets));
            Generators.Add(new ForestRadialGenerator(progress, assets));
            Generators.Add(new LakeSurfaceGenerator(assets));
            Generators.Add(new RocksGenerator(progress, assets));
            Generators.Add(new ScrubGenerator(progress, assets));
            Generators.Add(new ScrubRadialGenerator(progress, assets));
            Generators.Add(new TreesGenerator(progress, assets));
            Generators.Add(new TreeRowsGenerator(progress, assets));
            Generators.Add(new WatercourseGenerator(progress, assets));
            Generators.Add(new WatercourseRadialGenerator(progress, assets));
        }

        public List<ITerrainBuilderLayerGenerator> Generators { get; } = new List<ITerrainBuilderLayerGenerator>();
    }
}
