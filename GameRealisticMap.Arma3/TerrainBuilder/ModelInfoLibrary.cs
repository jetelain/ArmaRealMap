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
        private readonly Dictionary<string, ModelInfo> indexByName = new Dictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly IGameFileSystem fileSystem;

        public IEnumerable<ModelInfo> Models => indexByName.Values;

        public ModelInfoLibrary(IGameFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public ModelInfo ResolveByName(string name)
        {
            if (!indexByName.TryGetValue(name, out var modelInfo))
            {
                var candidates = fileSystem.FindAll(name + ".p3d").ToList();
                if (candidates.Count == 1)
                {
                    return ResolveByPath(candidates[0]);
                }
                if (candidates.Count > 1)
                {
                    throw new ApplicationException($"Name '{name}' matches multiples files : '{string.Join("', '", candidates)}'");
                }
                throw new ApplicationException($"Unknown model '{name}'");
            }
            return modelInfo;
        }

        public ModelInfo ResolveByPath(string path)
        {
            var model = indexByName.Values.FirstOrDefault(m => string.Equals(m.Path, path, StringComparison.OrdinalIgnoreCase));
            if (model == null)
            {
                var odol = ReadModelInfoOnly(path);
                if (odol == null)
                {
                    // TODO: Binarize on the fly
                    throw new ApplicationException($"ODOL file for model '{path}' was not found, unable to use it");
                }
                var name = UniqueName(Path.GetFileNameWithoutExtension(path));
                model = new ModelInfo(name, path, GetBundle(path), odol.BoundingCenter.Vector3);
                indexByName.Add(name, model);
            }
            return model;
        }

        private static string GetBundle(string path)
        {
            if (path.Length > 3 && path[1] == '\\')
            {
                return GetRootName(path.Substring(2));
            }
            if (path.Length > 4 && path[2] == '\\')
            {
                return path.Substring(0, 2) + "_" + GetRootName(path.Substring(3));
            }
            return GetRootName(path);
        }

        private static string GetRootName(string path)
        {
            var idx = path.IndexOf('\\');
            if ( idx != -1 )
            {
                return path.Substring(0, idx);
            }
            return path;
        }

        private string UniqueName(string initialName)
        {
            var name = initialName;
            var suffix = 1;
            while(indexByName.ContainsKey(name))
            {
                suffix++;
                name = FormattableString.Invariant($"{name}_{suffix}");
            }
            return name;
        }

        public BIS.P3D.ODOL.ModelInfo? ReadModelInfoOnly(string path)
        {
            using (var stream = fileSystem.OpenFileIfExists(path))
            {
                if (stream != null)
                {
                    var result = StreamHelper.Read<P3DInfosOnly>(stream).ModelInfo as BIS.P3D.ODOL.ModelInfo;
                    if (result != null)
                    {
                        return result;
                    }
                    // Mikero Tools binarize into project drive temp, binarized file might be there
                    using (var streamTemp = fileSystem.OpenFileIfExists("temp\\" + path))
                    {
                        if (streamTemp != null)
                        {
                            return StreamHelper.Read<P3DInfosOnly>(streamTemp).ModelInfo as BIS.P3D.ODOL.ModelInfo;
                        }
                    }
                }
            }
            return null;
        }

        public ODOL? ReadODOL(string path)
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
                    using (var streamTemp = fileSystem.OpenFileIfExists("temp\\" + path))
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
                    indexByName.Add(model.Name, model);
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
                    var odol = ReadModelInfoOnly(model.Path);
                    if (odol != null)
                    {
                        if (!IsAlmostSame(odol.BoundingCenter.Vector3, model.BoundingCenter))
                        {
                            updatedModel = new ModelInfo(model.Name, model.Path, model.Bundle, odol.BoundingCenter.Vector3);
                            updated++;
                        }
                    }
                    indexByName.Add(updatedModel.Name, updatedModel);
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
            var odol = ReadModelInfoOnly(model.Path);
            if (odol != null && !IsAlmostSame(odol.BoundingCenter.Vector3, model.BoundingCenter))
            {
                throw new ApplicationException($"BoundingCenter of '{model.Path}' mismatch: Database={model.BoundingCenter}, ODOL={odol.BoundingCenter.Vector3}.");
            }
        }

        private static bool IsAlmostSame(Vector3 a, Vector3 b)
        {
            return MathF.Abs(a.X - b.X) < 0.00001f
                && MathF.Abs(a.Y - b.Y) < 0.00001f 
                && MathF.Abs(a.Z - b.Z) < 0.00001f;
        }

        public async Task LoadFrom(string path)
        {
            using var stream = File.OpenRead(path);
            var items = await JsonSerializer.DeserializeAsync<List<ModelInfo>>(stream);
            indexByName.Clear();
            foreach(var item in items!)
            {
                indexByName[item.Name] = item;
            }
        }
        public async Task SaveTo(string path)
        {
            using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, indexByName.Values);
        }
    }
}
