namespace GameRealisticMap.Arma3.GameEngine.Roads
{
    public interface IArma3RoadTypeInfos
	{
        int Id { get; }
        float TextureWidth { get; }
        string Texture { get; }
        string TextureEnd { get; }
        string Material { get; }
        string Map { get; }
        float PathOffset { get; }
        bool IsPedestriansOnly { get; }
    }
}
