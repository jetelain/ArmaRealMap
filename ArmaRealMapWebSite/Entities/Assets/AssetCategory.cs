using System.ComponentModel.DataAnnotations;

namespace ArmaRealMapWebSite.Entities.Assets
{
    public enum AssetCategory
    {
        [Display(Name = "🌲 Arbre")]
        Tree,

        [Display(Name = "🌳 Buisson")]
        Bush,

        [Display(Name = "Sol (Clutter)")]
        Clutter,

        [Display(Name = "🌿 Plante")]
        Plant,

        [Display(Name = "🪨 Rocher")]
        Rock,

        [Display(Name = "🧱 Structure")]
        Structure,

        [Display(Name = "🏘 Batiment")]
        Building
    }
}