namespace GameRealisticMap.ElevationModel
{
    internal sealed class ElevationOutOfBoundsBuilder : IDataBuilder<ElevationOutOfBoundsData>
    {
        public ElevationOutOfBoundsData Build(IBuildContext context)
        {
            return new ElevationOutOfBoundsData(context.GetData<RawElevationData>().OutOfBounds);
        }
    }
}
