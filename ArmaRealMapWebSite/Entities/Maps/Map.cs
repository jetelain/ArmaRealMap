using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ArmaRealMap.Core.ObjectLibraries;
using CoordinateSharp;

namespace ArmaRealMapWebSite.Entities.Maps
{
    public class Map
    {
        public int MapID { get; set; }

        [Display(Name = "Nom technique")]
        [RegularExpression("^[a-zA-Z0-9_]+$")]
        public string Name { get; set; }

        [Display(Name = "Libellé")]
        public string Label { get; set; }

        [Display(Name = "Lien sur le Workshop")]
        public string Workshop { get; set; }

        [Display(Name = "Taille de la grille")]
        [Required]
        public int? GridSize { get; set; }

        [Display(Name = "Taille de cellule (m)")]
        [Required]
        public double? CellSize { get; set; }

        [Display(Name = "Résolution (m/px) des images")]
        [Required]
        public double? Resolution { get; set; }

        [Display(Name = "Région")]
        [Required]
        public TerrainRegion TerrainRegion { get; set; }

        [Display(Name = "Coordonnées MGRS du point Sud-Ouest")]
        public string MgrsBottomLeft { get; set; }

        [Display(Name = "Aperçu")]
        public string Preview { get; set; }

        [Display(Name = "Taille (Km)")]
        public double? SizeInKilometers => SizeInMeters / 1000;

        [Display(Name = "Taille (m)")]
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

    }
}
