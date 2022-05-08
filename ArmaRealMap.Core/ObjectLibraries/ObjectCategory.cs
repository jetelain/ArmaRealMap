using System.ComponentModel.DataAnnotations;

namespace ArmaRealMap.Core.ObjectLibraries
{
    public enum ObjectCategory
    {
        [Display(Name = "Batiment - Résidentiel")]
        Residential,

        [Display(Name = "Batiment - Industriel")]
        Industrial,

        [Display(Name = "Batiment - Commercial")]
        Retail,

        [Display(Name = "Batiment - Militaire")]
        Military,

        [Display(Name = "Batiment - Fort Historique")]
        HistoricalFort,

        [Display(Name = "Batiment - Eglise/Lieu de culte")]
        Church,

        [Display(Name = "Nature - Arbre isolé")]
        IsolatedTree,

        [Display(Name = "Nature - Bois/forêt")]
        ForestTree,

        [Display(Name = "Objet - Table de pic-nic")]
        PicnicTable,

        [Display(Name = "Objet - Banc")]
        Bench,

        [Display(Name = "Infrastructure - Mur d'autoroutes")]
        RoadConcreteWall,

        [Display(Name = "Nature - Bois/forêt - Lisière")]
        ForestEdge,

        [Display(Name = "Infrastructure - Trotoires")]
        RoadSideWalk,

        [Display(Name = "Batiment - Tour radio")]
        RadioTower,

        [Display(Name = "Infrastructure - Clotûre")]
        Fence,

        [Display(Name = "Infrastructure - Mur - Ville")]
        Wall,

        [Display(Name = "Objet - Puit (eau)")]
        WaterWell,

        [Display(Name = "Nature - Végétation aléatoire")]
        RandomVegetation,

        [Display(Name = "Nature - Bois/forêt - Objets additionels")]
        ForestAdditionalObjects,

        [Display(Name = "Nature - Brousailles")]
        Scrub,

        [Display(Name = "Nature - Falaises")]
        Cliff,

        [Display(Name = "Nature - Zone rocheuses")]
        GroundRock,

        [Display(Name = "Batiment - Hutte")]
        Hut,

        [Display(Name = "Infrastructure - Ponts - Routes principales / RN")]
        BridgePrimaryRoad,

        [Display(Name = "Infrastructure - Mur - Enceinte militaire")]
        MilitaryWall,

        [Display(Name = "Nature - Bois/forêt - Végétation radiale (25m)")]
        ForestRadialEdge,

        [Display(Name = "Nature - Brousailles - Objets additionels")]
        ScrubAdditionalObjects,

        [Display(Name = "Nature - Brousailles - Végétation radiale (12.5m)")]
        ScrubRadialEdge,

        [Display(Name = "Nature - Zone rocheuses - Objets additionels")]
        GroundRockAdditionalObjects,

        [Display(Name = "Nature - Bord des cours d'eau")]
        WaterwayBorder,

        [Display(Name = "Infrastructure - Ponts - Routes secondaires / RD")]
        BridgeSecondaryRoad,

        [Display(Name = "Infrastructure - Ponts - Routes en béton")]
        BridgeConcreteRoad,
    }
}
