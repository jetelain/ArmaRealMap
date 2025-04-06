using System.Diagnostics.CodeAnalysis;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Arma3.Test.Edit
{
    internal class ModelInfoLibraryMock : IModelInfoLibrary
    {
        public bool? IsSlopeLandContact(string path)
        {
            if (path.Contains("slopelandcontact", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (path.Contains("notfound"))
            {
                return null;
            }
            return false;
        }

        public ModelInfo ResolveByName(string name)
        {
            return new ModelInfo(name, name + ".p3d", new System.Numerics.Vector3());
        }

        public ModelInfo ResolveByPath(string path)
        {
            return new ModelInfo(Path.GetFileNameWithoutExtension(path.Replace('\\', Path.DirectorySeparatorChar)), path, new System.Numerics.Vector3());
        }

        public string? TryGetNoLandContact(string path)
        {
            return null;
        }

        public bool TryRegister(string name, string path)
        {
            throw new NotImplementedException();
        }

        public bool TryResolveByName(string name, [MaybeNullWhen(false)] out ModelInfo model)
        {
            throw new NotImplementedException();
        }

        public bool TryResolveByPath(string path, [MaybeNullWhen(false)] out ModelInfo model)
        {
            throw new NotImplementedException();
        }
    }
}