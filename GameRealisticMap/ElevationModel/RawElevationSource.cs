using MapToolkit.DataCells;

namespace GameRealisticMap.ElevationModel
{
    internal class RawElevationSource
    {
        
        public RawElevationSource(List<string> dbCredits, IDemDataCell view, IDemDataCell viewFull)
        {
            this.Credits = dbCredits;
            this.SurfaceOnly = view;
            this.Ground = viewFull;
        }

        public List<string> Credits { get; }
        public IDemDataCell SurfaceOnly { get; }
        public IDemDataCell Ground { get; }
    }
}