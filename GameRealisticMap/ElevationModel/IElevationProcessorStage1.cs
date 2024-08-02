using Pmad.ProgressTracking;

namespace GameRealisticMap.ElevationModel
{
    internal interface IElevationProcessorStage1
    {
        void ProcessStage1(ElevationGrid grid, IContext context, IProgressScope scope);
    }
}
