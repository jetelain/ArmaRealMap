namespace GameRealisticMap.Arma3.GameEngine.Roads
{
    public sealed class EditableArma3RoadTypeInfos : IArma3RoadTypeInfos
    {
        public EditableArma3RoadTypeInfos(int id, float textureWidth, string texture, string textureEnd, string material, string map, float pathOffset, bool isPedestriansOnly)
        {
            Id = id;
            TextureWidth = textureWidth;
            Texture = texture;
            TextureEnd = textureEnd;
            Material = material;
            Map = map;
            PathOffset = pathOffset;
            IsPedestriansOnly = isPedestriansOnly;
        }

        public int Id { get; set; }

        public float TextureWidth { get; set; }

        public string Texture { get; set; }

        public string TextureEnd { get; set; }

        public string Material { get; set; }

        public string Map { get; set; }

        public float PathOffset { get; set; }

        public bool IsPedestriansOnly { get; set; }
    }
}
