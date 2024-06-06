using GameRealisticMap.ElevationModel;
using MapToolkit.DataCells;

namespace GameRealisticMap.Generic.Exporters.ElevationModel
{
    internal class ElevationRawExporter : ElevationExporterBase
    {
        public override string Name => "ElevationRaw";

        protected override ElevationGrid GetDataCell(IBuildContext context)
        {
            return context.GetData<RawElevationData>().RawElevation;
        }
    }
}
