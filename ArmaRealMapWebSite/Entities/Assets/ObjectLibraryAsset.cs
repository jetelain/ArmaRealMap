using System.ComponentModel.DataAnnotations;

namespace ArmaRealMapWebSite.Entities.Assets
{
    public class ObjectLibraryAsset
    {
        public int ObjectLibraryAssetID { get; set; }

        public int ObjectLibraryID { get; set; }
        public ObjectLibrary ObjectLibrary { get; set; }

        public int AssetID { get; set; }
        public Asset Asset { get; set; }

        [Display(Name = "Probabilité")]
        [Range(0, 1)]
        public float? Probability { get; set; }

        [Display(Name = "Emprise de placement (rayon du centre de l'objet)")]
        public float? PlacementRadius { get; set; }

        [Display(Name = "Distance de la bordure de zone (rayon du centre de l'objet)")]
        public float? ReservedRadius { get; set; }

        [Display(Name = "Décalage maximum de l'altitude")]
        public float? MaxZ { get; set; }

        [Display(Name = "Décalage minimum de l'altitude")]
        public float? MinZ { get; set; }
    }
}