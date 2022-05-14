using System;
using System.ComponentModel.DataAnnotations;
using ArmaRealMap.Core;
using CoordinateSharp;

namespace ArmaRealMapWebSite.Entities.Maps
{
    public class Map
    {
        public int MapID { get; set; }

        [Display(Name = "Technical name")]
        [RegularExpression("^[a-zA-Z0-9_]+$")]
        public string Name { get; set; }

        [Display(Name = "Label")]
        public string Label { get; set; }

        [Display(Name = "Workshop link")]
        public string Workshop { get; set; }

        [Display(Name = "Grid size")]
        [Required]
        public int? GridSize { get; set; }

        [Display(Name = "Cell size (m)")]
        [Required]
        public double? CellSize { get; set; }

        [Display(Name = "Images resolution (m/px)")]
        [Required]
        public double? Resolution { get; set; }

        [Display(Name = "Region")]
        [Required]
        public TerrainRegion TerrainRegion { get; set; }

        [Display(Name = "MGRS coordinates of South-West point")]
        public string MgrsBottomLeft { get; set; }

        [Display(Name = "Preview")]
        public string Preview { get; set; }

        [Display(Name = "Size (Km)")]
        public double? SizeInKilometers => SizeInMeters / 1000;

        [Display(Name = "Size (m)")]
        public double? SizeInMeters => GridSize * CellSize;

        public Coordinate FromGameCoordinates(double x, double y)
        {
            var southWest = MilitaryGridReferenceSystem.MGRStoLatLong(MilitaryGridReferenceSystem.Parse(MgrsBottomLeft));

            var startPointUTM = new UniversalTransverseMercator(
                southWest.UTM.LatZone,
                southWest.UTM.LongZone,
                Math.Round(southWest.UTM.Easting),
                Math.Round(southWest.UTM.Northing));

            var utm = new UniversalTransverseMercator(
                        startPointUTM.LatZone,
                        startPointUTM.LongZone,
                        startPointUTM.Easting + x,
                        startPointUTM.Northing + y);

            return UniversalTransverseMercator.ConvertUTMtoLatLong(utm, new EagerLoad(false));
        }

        public double[][] ToLeafletRectangle()
        {
            var SW = FromGameCoordinates(0, 0);
            var SE = FromGameCoordinates(0, SizeInMeters.Value);
            var NW = FromGameCoordinates(SizeInMeters.Value, 0);
            var NE = FromGameCoordinates(SizeInMeters.Value, SizeInMeters.Value);
            return new[] {
                new[] {@SW.Latitude.ToDouble(), @SW.Longitude.ToDouble()},
                new[] {@SE.Latitude.ToDouble(), @SE.Longitude.ToDouble()},
                new[] {@NE.Latitude.ToDouble(), @NE.Longitude.ToDouble()},
                new[] {@NW.Latitude.ToDouble(), @NW.Longitude.ToDouble()}
            };
        }

    }
}
