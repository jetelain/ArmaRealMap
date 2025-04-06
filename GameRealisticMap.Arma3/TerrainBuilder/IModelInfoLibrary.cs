using System.Diagnostics.CodeAnalysis;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    public interface IModelInfoLibrary
    {
        ModelInfo ResolveByName(string name);

        bool TryResolveByName(string name, [MaybeNullWhen(false)] out ModelInfo model);

        ModelInfo ResolveByPath(string path);

        bool TryResolveByPath(string path, [MaybeNullWhen(false)] out ModelInfo model);

        bool TryRegister(string name, string path);

        bool? IsSlopeLandContact(string model);
    }
}
