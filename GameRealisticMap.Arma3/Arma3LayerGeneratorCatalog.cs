using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.ManMade;
using GameRealisticMap.Arma3.ManMade.Farmlands;
using GameRealisticMap.Arma3.Nature.DefaultAreas;
using GameRealisticMap.Arma3.Nature.DefaultUrbanAreas;
using GameRealisticMap.Arma3.Nature.Forests;
using GameRealisticMap.Arma3.Nature.Lakes;
using GameRealisticMap.Arma3.Nature.RockAreas;
using GameRealisticMap.Arma3.Nature.Scrubs;
using GameRealisticMap.Arma3.Nature.Trees;
using GameRealisticMap.Arma3.Nature.Watercourses;
using GameRealisticMap.Reporting;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3
{
    /// <summary>
    /// Registry of all <see cref="ITerrainBuilderLayerGenerator"/> instances for a map generation run.
    /// Instantiate with an <see cref="IArma3RegionAssets"/> to get all generators pre-configured
    /// for the target region's asset library.
    /// </summary>
    public class Arma3LayerGeneratorCatalog
    {
        /// <summary>
        /// Initialises the catalog and registers all standard layer generators for both
        /// man-made features (buildings, roads, railways, fences, …) and nature
        /// (forests, scrubs, lakes, rocks, trees, watercourses, …).
        /// </summary>
        /// <param name="assets">Region-specific asset library providing 3D models and terrain materials.</param>
        public Arma3LayerGeneratorCatalog(IArma3RegionAssets assets)
        {
            // All generators

            // ManMade
            Generators.Add(new BuildingGenerator(assets));
            Generators.Add(new OrientedObjectsGenerator(assets));
            Generators.Add(new BridgeGenerator(assets));
            Generators.Add(new FenceGenerator(assets));
            Generators.Add(new RailwayGenerator(assets));
            Generators.Add(new VineyardsGenerator(assets));
            Generators.Add(new OrchardGenerator(assets));
            Generators.Add(new SidewalksGenerator(assets));
            Generators.Add(new DefaultAgriculturalAreasGenerator(assets));
            Generators.Add(new DefaultCommercialAreasGenerator(assets));
            Generators.Add(new DefaultIndustrialAreasGenerator(assets));
            Generators.Add(new DefaultMilitaryAreasGenerator(assets));
            Generators.Add(new DefaultResidentialAreasGenerator(assets));
            Generators.Add(new DefaultRetailAreasGenerator(assets));

            // Nature
            Generators.Add(new ForestEdgeGenerator(assets));
            Generators.Add(new ForestGenerator(assets));
            Generators.Add(new ForestRadialGenerator(assets));
            Generators.Add(new LakeSurfaceGenerator(assets));
            Generators.Add(new RocksGenerator(assets));
            Generators.Add(new ScreeGenerator(assets));
            Generators.Add(new ScrubGenerator(assets));
            Generators.Add(new ScrubRadialGenerator(assets));
            Generators.Add(new TreesGenerator(assets));
            Generators.Add(new TreeRowsGenerator(assets));
            Generators.Add(new WatercourseGenerator(assets));
            Generators.Add(new WatercourseRadialGenerator(assets));
            Generators.Add(new DefaultAreasGenerator(assets));
        }

        public List<ITerrainBuilderLayerGenerator> Generators { get; } = new List<ITerrainBuilderLayerGenerator>();
    }
}
