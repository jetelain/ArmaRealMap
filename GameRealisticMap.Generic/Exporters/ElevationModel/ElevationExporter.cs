using GameRealisticMap.ElevationModel;
using Pmad.Cartography.DataCells;

namespace GameRealisticMap.Generic.Exporters.ElevationModel
{
    internal class ElevationExporter : ElevationExporterBase
    {
        public override string Name => "Elevation";

        protected override ElevationGrid GetDataCell(IBuildContext context)
        {
            return context.GetData<ElevationData>().Elevation;
        }
    }
}
