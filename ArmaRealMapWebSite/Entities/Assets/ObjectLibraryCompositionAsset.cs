using System.ComponentModel.DataAnnotations;

namespace ArmaRealMapWebSite.Entities.Assets
{
    public class ObjectLibraryCompositionAsset
    {
        public int ObjectLibraryCompositionAssetID { get; set; }

        public int ObjectLibraryCompositionID { get; set; }
        public ObjectLibraryComposition Composition { get; set; }

        public int AssetID { get; set; }
        public Asset Asset { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Angle { get; set; }
    }
}