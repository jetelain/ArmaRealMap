using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using ArmaRealMap.Core.ObjectLibraries;

namespace ArmaRealMapWebSite.Entities.Assets
{
    public class Asset
    {
        public int AssetID { get; set; }

        [Display(Name = "Modèle 3D")]
        public string Name { get; set; }

        [Display(Name = "Configuration")]
        public string ClassName { get; set; }
        [Display(Name = "Chemin P3D")]
        public string ModelPath { get; set; }
        public float Width { get; set; }
        public float Depth { get; set; }
        public float Height { get; set; }
        public float CX { get; set; }
        public float CY { get; set; }
        public float CZ { get; set; }
        public List<AssetPreview> Previews { get; set; }

        [Display(Name = "Régions")]
        public TerrainRegion TerrainRegions { get; set; }

        [Display(Name = "Catégorie")]
        public AssetCategory AssetCategory { get; set; }
        public string TerrainBuilderTemplateXML { get; set; }


        [Display(Name = "Mod")]
        public GameMod GameMod { get; set; }
        [Display(Name = "Mod")]
        public int GameModID { get; set; }

        public float MaxZ { get; set; }
        public float MaxY { get; set; }
        public float MaxX { get; set; }
        public float MinZ { get; set; }
        public float MinY { get; set; }
        public float MinX { get; set; }

        [Display(Name = "Taille (m)")]
        public float BoundingSphereDiameter { get; set; }

        public string ClusterName { get; set; }

        public float? BoundingCenterX { get; set; }
        public float? BoundingCenterY { get; set; }
        public float? BoundingCenterZ { get; set; }
    }
}
