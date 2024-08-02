using Pmad.ProgressTracking;

namespace GameRealisticMap.ElevationModel
{
    internal sealed class ElevationOutOfBoundsBuilder : IDataBuilder<ElevationOutOfBoundsData>
    {
        public ElevationOutOfBoundsData Build(IBuildContext context, IProgressScope scope)
        {
            return new ElevationOutOfBoundsData(context.GetData<RawElevationData>().OutOfBounds);
        }
    }
}
