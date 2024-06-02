using GameRealisticMap.ElevationModel;
using MapToolkit.DataCells;

namespace GameRealisticMap.Generic.Exporters.ElevationModel
{
    internal class ElevationStampedExporter : ElevationExporterBase
    {
        public override string Name => "ElevationStamped";

        protected override DemDataCellPixelIsPoint<float> GetDataCell(IBuildContext context)
        {
            return context.GetData<ElevationWithLakesData>().Elevation.ToDataCell();
        }
    }
}
