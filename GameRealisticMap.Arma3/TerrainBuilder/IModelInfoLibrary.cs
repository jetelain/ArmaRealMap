using System.Diagnostics.CodeAnalysis;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    public interface IModelInfoLibrary
    {
        ModelInfo ResolveByName(string name);

        ModelInfo ResolveByPath(string path);

        bool TryResolveByPath(string path, [MaybeNullWhen(false)] out ModelInfo model);
    }
}
