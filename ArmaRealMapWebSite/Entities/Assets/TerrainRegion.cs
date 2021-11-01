using System;
using System.ComponentModel.DataAnnotations;

namespace ArmaRealMapWebSite.Entities.Assets
{
    //[Flags]
    public enum TerrainRegion
    {
        [Display(Name = "Non précisé")]
        Unknown       = 0x00,

        [Display(Name = "Europe centrale")]
        CentralEurope = 0x01,

        [Display(Name = "Sahel")]
        Sahel         = 0x02,

        [Display(Name = "Méditerranéen")]
        Mediterranean = 0x04,

        [Display(Name = "Tropical")]
        Tropical      = 0x08,

        [Display(Name = "Proche orient")]
        NearEast      = 0x10
    }
}
