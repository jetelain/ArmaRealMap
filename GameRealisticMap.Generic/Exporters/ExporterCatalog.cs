using GameRealisticMap.Generic.Exporters.ElevationModel;
using GameRealisticMap.Generic.Exporters.ManMade;
using GameRealisticMap.Generic.Exporters.Nature;
using GameRealisticMap.Generic.Exporters.Satellite;
using GameRealisticMap.ManMade.Airports;
using GameRealisticMap.ManMade.DefaultUrbanAreas;
using GameRealisticMap.ManMade.Farmlands;
using GameRealisticMap.ManMade.Surfaces;
using GameRealisticMap.Nature.DefaultAreas;
using GameRealisticMap.Nature.Forests;
using GameRealisticMap.Nature.Lakes;
using GameRealisticMap.Nature.Ocean;
using GameRealisticMap.Nature.RockAreas;
using GameRealisticMap.Nature.Scrubs;
using GameRealisticMap.Nature.Surfaces;
using GameRealisticMap.Nature.Watercourses;

namespace GameRealisticMap.Generic.Exporters
{
    public class ExporterCatalog
    {
        private readonly List<IExporter> exporters = new List<IExporter>();

        public ExporterCatalog()
        {
            exporters.Add(new ElevationRawExporter());
            exporters.Add(new ElevationStampedExporter());
            exporters.Add(new ElevationExporter());
            exporters.Add(new RawSatelliteImageExporter());
            exporters.Add(new BasicTerrainExporter<AsphaltData>());
            exporters.Add(new BasicTerrainExporter<CoastlineData>());
            exporters.Add(new BasicTerrainExporter<FarmlandsData>());
            exporters.Add(new BasicTerrainExporter<ForestData>());
            exporters.Add(new BasicTerrainExporter<ForestEdgeData>());
            exporters.Add(new BasicTerrainExporter<ForestRadialData>());
            exporters.Add(new BasicTerrainExporter<GrassData>());
            exporters.Add(new BasicTerrainExporter<IceSurfaceData>("Ice"));
            exporters.Add(new BasicTerrainExporter<LakesData>("LakesRaw"));
            exporters.Add(new BasicTerrainExporter<MeadowsData>());
            exporters.Add(new BasicTerrainExporter<OceanData>());
            exporters.Add(new BasicTerrainExporter<OrchardData>());
            exporters.Add(new BasicTerrainExporter<RocksData>());
            exporters.Add(new BasicTerrainExporter<SandSurfacesData>("Sand"));
            exporters.Add(new BasicTerrainExporter<ScreeData>());
            exporters.Add(new BasicTerrainExporter<ScrubData>());
            exporters.Add(new BasicTerrainExporter<ScrubRadialData>());
            exporters.Add(new BasicTerrainExporter<VineyardData>());
            exporters.Add(new BasicTerrainExporter<WatercourseRadialData>());
            exporters.Add(new BasicTerrainExporter<WatercoursesData>("WatercourseSurface"));
            exporters.Add(new BasicTerrainExporter<DefaultAgriculturalAreaData>());
            exporters.Add(new BasicTerrainExporter<DefaultCommercialAreaData>());
            exporters.Add(new BasicTerrainExporter<DefaultIndustrialAreaData>());
            exporters.Add(new BasicTerrainExporter<DefaultMilitaryAreaData>());
            exporters.Add(new BasicTerrainExporter<DefaultResidentialAreaData>());
            exporters.Add(new BasicTerrainExporter<DefaultRetailAreaData>());
            exporters.Add(new BasicTerrainExporter<DefaultAreasData>());
            exporters.Add(new BasicTerrainExporter<AirportData>());
            exporters.Add(new LakesExporter());
            exporters.Add(new BuildingsRectangleExporter());
            exporters.Add(new BuildingsRealShapeExporter());
            exporters.Add(new FencesExporter());
            exporters.Add(new RailwaysExporter());
            exporters.Add(new RoadsExporter());
        }

        public IEnumerable<IExporterInfo> Exporters => exporters.Cast<IExporterInfo>();

        internal IExporter Get(string name)
        {
            return exporters.FirstOrDefault(e => e.Name == name) ?? throw new ApplicationException($"Export '{name}' does not exists.");
        }
    }
}
