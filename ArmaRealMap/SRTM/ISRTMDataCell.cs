namespace SRTM
{
    public interface ISRTMDataCell
    {
        int Latitude { get; }

        int Longitude { get; }

        int? GetElevation(double latitude, double longitude);
        
        double? GetElevationBilinear(double latitude, double longitude);
    }
}
