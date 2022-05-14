using System;
using System.ComponentModel.DataAnnotations;

namespace ArmaRealMap.Core
{
    public enum TerrainRegion
    {
        [Display(Name = "Not specified")]
        Unknown       = 0x00,

        [Display(Name = "Central Europe")]
        CentralEurope = 0x01,

        [Display(Name = "Sahel")]
        Sahel         = 0x02,

        [Display(Name = "Mediterranean")]
        Mediterranean = 0x04,

        [Display(Name = "Tropical")]
        Tropical      = 0x08,

        [Display(Name = "Near east")]
        NearEast      = 0x10
    }
}
