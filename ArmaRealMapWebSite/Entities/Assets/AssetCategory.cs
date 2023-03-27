using System.ComponentModel.DataAnnotations;

namespace ArmaRealMapWebSite.Entities.Assets
{
    public enum AssetCategory
    {
        [Display(Name = "Nature - 🌲 Arbre")]
        Tree,

        [Display(Name = "Nature - 🌳 Buisson")]
        Bush,

        [Display(Name = "Nature - Sol (Clutter)")]
        Clutter,

        [Display(Name = "Nature - 🌿 Plante")]
        Plant,

        [Display(Name = "Nature - 🪨 Rocher")]
        Rock,

        [Display(Name = "🧱 Structure")]
        Structure,

        [Display(Name = "Batiment -  🏘(générique)")]
        Building,

        [Display(Name = "Nature - 🌅 Eau")]
        Water,

        [Display(Name = "Batiment - 🏚 Ruines")]
        Ruins,

        [Display(Name = "Batiment - 🏠 Résidentiel")]
        Residential,

        [Display(Name = "Batiment - 🏭 Industriel")]
        Industrial,

        [Display(Name = "Batiment - 🏢 Commercial")]
        Retail,

        [Display(Name = "Batiment - 🪖 Militaire")]
        Military,

        [Display(Name = "Batiment - 🏰 Fort Historique")]
        HistoricalFort,

        [Display(Name = "Batiment - ⛪ Eglise/Lieu de culte")]
        Church,

        [Display(Name = "Batiment - 🗼 Tour radio")]
        RadioTower, 
        
        [Display(Name = "Batiment - 🛖 Hutte")]
        Hut,

        [Display(Name = "Batiment - Chateau d'eau")]
        WaterTower,

        [Display(Name = "Batiment - 🚜 Agricole")]
        Farm,
    }
}