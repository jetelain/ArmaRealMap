using GeoAPI.Geometries;
using MapToolkit;
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

        internal float GetElevation(Coordinate latLong, byte oceanMaskValue)
        {
            if (oceanMaskValue == 0)
            {
                return (float)GetSurfaceElevation(latLong);
            }
            if ( oceanMaskValue == 255)
            {
                return (float)GetOceanDepth(latLong);
            }
            var factor = oceanMaskValue / 255.0;
            return (float)((GetOceanDepth(latLong) * factor) + (GetSurfaceElevation(latLong) * (1 - factor)));
        }

        private double GetOceanDepth(Coordinate latLong)
        {
            var elevation = GetElevationBilinear(Ground, latLong.Y, latLong.X);
            if (elevation > -1)
            {
                return -1;
            }
            return elevation;
        }

        private double GetSurfaceElevation(Coordinate latLong)
        {
            var elevation = GetElevationBilinear(SurfaceOnly, latLong.Y, latLong.X);
            if (double.IsNaN(elevation) || elevation < 0.5f)
            {
                elevation = 0.5f;
            }
            return elevation;
        }

        private double GetElevationBilinear(IDemDataCell view, double lat, double lon)
        {
            return view.GetLocalElevation(new Coordinates(lat, lon), DefaultInterpolation.Instance);
        }
    }
}