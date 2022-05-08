using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ArmaRealMap.Core;
using ArmaRealMap.Core.ObjectLibraries;
using ArmaRealMapWebSite.Entities.Assets;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ArmaRealMapWebSite.Models
{
    public class AssetsViewModel
    {
        public List<Asset> Results { get; internal set; }
        public SelectList Mods { get; internal set; }

        [Display(Name = "Régions")]
        public TerrainRegion? TerrainRegion { get; set; }

        [Display(Name = "Catégorie")]
        public AssetCategory? AssetCategory { get; set; }

        [Display(Name = "Mod")]
        public int? GameModID { get; set; }

        [Display(Name = "Nom")]
        public string Name { get; set; }
        public int DbCount { get; set; }
    }
}
