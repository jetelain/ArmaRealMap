using System;
using System.ComponentModel.DataAnnotations;

namespace ArmaRealMap.Core
{
    public enum TerrainRegion
    {
        [Display(Name = "Not specified")]
        Unknown       = 0x000,

        [Display(Name = "Central Europe")]
        CentralEurope = 0x001,

        [Display(Name = "Sahel")]
        Sahel         = 0x002,

        [Display(Name = "Mediterranean")]
        Mediterranean = 0x004,

        [Display(Name = "Tropical")]
        Tropical      = 0x008,

        [Display(Name = "Near east")]
        NearEast      = 0x010,

        [Display(Name = "North america")]
        NorthAmerica  = 0x020,

        [Display(Name = "East europe")]
        EastEurope    = 0x040,

        [Display(Name = "West europe")]
        WestEurope    = 0x100
    }
}
