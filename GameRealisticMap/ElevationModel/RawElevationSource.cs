using GeoAPI.Geometries;
using Pmad.Cartography;
using Pmad.Cartography.Databases;
using Pmad.Cartography.DataCells;

namespace GameRealisticMap.ElevationModel
{
    internal class RawElevationSource
    {
        private readonly IDemDataCell surfaceOnly;
        private readonly IDemDataCell ground;

        public RawElevationSource(List<string> dbCredits, IDemDataCell view, IDemDataCell viewFull)
        {
            this.Credits = dbCredits;
            this.surfaceOnly = view;
            this.ground = viewFull;
        }

        public List<string> Credits { get; }

        internal float GetElevation(Coordinate latLong, byte oceanMaskValue)
        {
            if (oceanMaskValue == 0)
            {
                return (float)GetSurfaceElevation(latLong);
            }
            if (oceanMaskValue == 255)
            {
                return (float)GetOceanDepth(latLong);
            }
            var factor = oceanMaskValue / 255.0;
            return (float)((GetOceanDepth(latLong) * factor) + (GetSurfaceElevation(latLong) * (1 - factor)));
        }

        private double GetOceanDepth(Coordinate latLong)
        {
            var elevation = GetElevationBilinear(ground, latLong.Y, latLong.X);
            if (elevation > -1)
            {
                return -1;
            }
            return elevation;
        }

        private double GetSurfaceElevation(Coordinate latLong)
        {
            var elevation = GetElevationBilinear(surfaceOnly, latLong.Y, latLong.X);
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

        internal double GetElevationNoMask(Coordinate latLong)
        {
            var point = new Coordinates(latLong.Y, latLong.X);
            var detail = surfaceOnly.GetLocalElevation(point, DefaultInterpolation.Instance);
            if (double.IsNaN(detail) || Math.Abs(detail) < 0.1)
            {
                return ground.GetLocalElevation(point, DefaultInterpolation.Instance);
            }
            return detail;
        }
    }
}