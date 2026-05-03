using System.Diagnostics.CodeAnalysis;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    /// <summary>
    /// Provides lookup of <see cref="ModelInfo"/> by model name or P3D path.
    /// Implemented by <see cref="ModelInfoLibrary"/> which scans the Arma 3 P:\ drive.
    /// </summary>
    public interface IModelInfoLibrary
    {
        ModelInfo ResolveByName(string name);

        bool TryResolveByName(string name, [MaybeNullWhen(false)] out ModelInfo model);

        ModelInfo ResolveByPath(string path);

        bool TryResolveByPath(string path, [MaybeNullWhen(false)] out ModelInfo model);

        bool TryRegister(string name, string path);

        bool? IsSlopeLandContact(string path);

        string? TryGetNoLandContact(string path);
    }
}
