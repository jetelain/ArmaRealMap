namespace GameRealisticMap.Arma3.TerrainBuilder
{
    public interface IModelInfoLibrary
    {
        ModelInfo ResolveByName(string name);

        ModelInfo ResolveByPath(string path);
    }
}
