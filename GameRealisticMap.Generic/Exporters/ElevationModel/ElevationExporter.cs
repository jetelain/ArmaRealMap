using GameRealisticMap.ElevationModel;
using MapToolkit.DataCells;

namespace GameRealisticMap.Generic.Exporters.ElevationModel
{
    internal class ElevationExporter : ElevationExporterBase
    {
        public override string Name => "Elevation";

        protected override DemDataCellPixelIsPoint<float> GetDataCell(IBuildContext context)
        {
            return context.GetData<ElevationData>().Elevation.ToDataCell();
        }
    }
}
