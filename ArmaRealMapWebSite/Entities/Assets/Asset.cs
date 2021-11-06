using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public GameMod GameMod { get; internal set; }
        [Display(Name = "Mod")]
        public int GameModID { get; set; }

        public float MaxZ { get; internal set; }
        public float MaxY { get; internal set; }
        public float MaxX { get; internal set; }
        public float MinZ { get; internal set; }
        public float MinY { get; internal set; }
        public float MinX { get; internal set; }

        [Display(Name = "Taille (m)")]
        public float BoundingSphereDiameter { get; internal set; }
    }
}
