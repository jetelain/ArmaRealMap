using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ArmaRealMap.Core;
using ArmaRealMap.Core.ObjectLibraries;

namespace ArmaRealMapWebSite.Entities.Assets
{
    public class ObjectLibrary
    {
        public int ObjectLibraryID { get; set; }

        [Display(Name = "Libellé")]
        public string Name { get; set; }

        [Display(Name = "Région")]
        public TerrainRegion TerrainRegion { get; set; }

        [Display(Name = "Type")]
        public ObjectCategory ObjectCategory { get; set; }

        [Display(Name = "Densitée")]
        public double? Density { get; set; }

        [Display(Name = "Probabilité")]
        [Range(0, 1)]
        public double? Probability { get; set; }

        public List<ObjectLibraryAsset> Assets { get; set; }
        public List<ObjectLibraryComposition> Compositions { get; set; }
    }
}
