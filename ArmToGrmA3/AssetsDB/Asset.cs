using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using ArmaRealMap.Core;
using ArmaRealMap.Core.ObjectLibraries;

namespace ArmaRealMapWebSite.Entities.Assets
{
    public class Asset
    {
        public int AssetID { get; set; }

        [Display(Name = "Modèle 3D")]
        public string Name { get; set; }

        [Display(Name = "Chemin P3D")]
        public string ModelPath { get; set; }

        public List<AssetPreview> Previews { get; set; }

        [Display(Name = "Catégorie")]
        public AssetCategory AssetCategory { get; set; }

    }
}
