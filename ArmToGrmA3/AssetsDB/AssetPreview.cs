namespace ArmaRealMapWebSite.Entities.Assets
{
    public class AssetPreview
    {
        public int AssetPreviewID { get; set; }
        public int AssetID { get; set; }
        public Asset Asset { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] Data { get; set; }
    }
}
