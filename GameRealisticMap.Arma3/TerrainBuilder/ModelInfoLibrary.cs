using System.Numerics;
using System.Text.Json;
using BIS.Core.Streams;
using BIS.P3D;
using BIS.P3D.ODOL;
using GameRealisticMap.Arma3.IO;

namespace GameRealisticMap.Arma3.TerrainBuilder
{
    public class ModelInfoLibrary : IModelInfoLibrary
    {
        private readonly Dictionary<string, ModelInfo> models = new Dictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly IGameFileSystem fileSystem;

        public IEnumerable<ModelInfo> Models => models.Values;

        public ModelInfoLibrary(IGameFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public ModelInfo ResolveByName(string name)
        {
            if (!models.TryGetValue(name, out var modelInfo))
            {
                throw new ApplicationException($"Unknown model '{name}'");
            }
            return modelInfo;
        }

        public ModelInfo ResolveByPath(string path)
        {
            var model = models.Values.FirstOrDefault(m => string.Equals(m.Path, path, StringComparison.OrdinalIgnoreCase));
            if (model == null)
            {
                var odol = ReadODOL(path);
                if (odol == null)
                {
                    throw new ApplicationException($"ODOL file for model '{path}' was not found, unable to use it");
                }
                var name = UniqueName(Path.GetFileNameWithoutExtension(path));
                model = new ModelInfo(name, path, "unknown", odol.ModelInfo.BoundingCenter.Vector3);
                models.Add(name, model);
            }
            return model;
        }

        private string UniqueName(string initialName)
        {
            var name = initialName;
            var suffix = 1;
            while(models.ContainsKey(name))
            {
                suffix++;
                name = FormattableString.Invariant($"{name}_{suffix}");
            }
            return name;
        }

        private ODOL? ReadODOL(string path)
        { 
            using (var stream = fileSystem.OpenFileIfExists(path))
            {
                if (stream != null)
                {
                    if (P3D.IsODOL(stream))
                    {
                        return StreamHelper.Read<ODOL>(stream);
                    }

                    // Mikero Tools binarize into project drive temp, binarized file might be there
                    using (var streamTemp = fileSystem.OpenFileIfExists(Path.Combine("temp", path)))
                    {
                        if (streamTemp != null && P3D.IsODOL(streamTemp))
                        {
                            return StreamHelper.Read<ODOL>(streamTemp);
                        }
                    }
                }
            }
            return null;
        }

        internal async Task Load(Stream stream)
        {
            await foreach (var model in JsonSerializer.DeserializeAsyncEnumerable<ModelInfo>(stream))
            {
                if (model != null)
                {
                    CheckBoudingCenter(model);
                    models.Add(model.Name, model);
                }
            }
        }

        internal async ValueTask<bool> LoadAndUpdate(Stream stream)
        {
            var updated = 0;
            await foreach (var model in JsonSerializer.DeserializeAsyncEnumerable<ModelInfo>(stream))
            {
                if (model != null)
                {
                    var updatedModel = model;
                    var odol = ReadODOL(model.Path);
                    if (odol != null)
                    {
                        if (!IsAlmostSame(odol.ModelInfo.BoundingCenter.Vector3, model.BoundingCenter))
                        {
                            updatedModel = new ModelInfo(model.Name, model.Path, model.Bundle, odol.ModelInfo.BoundingCenter.Vector3);
                            updated++;
                        }
                    }
                    models.Add(updatedModel.Name, updatedModel);
                }
            }
            return updated > 0;
        }

        internal async Task Save(Stream stream)
        {
            await JsonSerializer.SerializeAsync(stream, Models);
        }

        private void CheckBoudingCenter(ModelInfo model)
        {
            var odol = ReadODOL(model.Path);
            if (odol != null && !IsAlmostSame(odol.ModelInfo.BoundingCenter.Vector3, model.BoundingCenter))
            {
                throw new ApplicationException($"BoundingCenter of '{model.Path}' mismatch: Database={model.BoundingCenter}, ODOL={odol.ModelInfo.BoundingCenter.Vector3}.");
            }
        }

        private static bool IsAlmostSame(Vector3 a, Vector3 b)
        {
            return MathF.Abs(a.X - b.X) < 0.00001f
                && MathF.Abs(a.Y - b.Y) < 0.00001f 
                && MathF.Abs(a.Z - b.Z) < 0.00001f;
        }
    }
}
