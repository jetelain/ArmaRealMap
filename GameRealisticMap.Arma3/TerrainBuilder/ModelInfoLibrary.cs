using System.Diagnostics.CodeAnalysis;
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
        public static string DefaultCachePath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GameRealisticMap", "Arma3", "modelinfo.json");

        private readonly Dictionary<string, bool?> slopelandcontact = new Dictionary<string, bool?>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ModelInfo> indexByName = new Dictionary<string, ModelInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly IGameFileSystem fileSystem;

        public IEnumerable<ModelInfo> Models => indexByName.Values;

        public ModelInfoLibrary(IGameFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public bool TryResolveByName(string name, [MaybeNullWhen(false)] out ModelInfo model)
        {
            return indexByName.TryGetValue(name, out model);
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
                    throw new AmbiguousModelName(name, candidates);
                }
                throw new ApplicationException($"Unknown model '{name}'");
            }
            return modelInfo;
        }

        public ModelInfo ResolveByPath(string path)
        {
            if (!TryResolveByPath(path, out var model))
            {
                throw new ApplicationException($"ODOL file for model '{path}' was not found, unable to use it");
            }
            return model;
        }

        public bool TryResolveByPath(string path, [MaybeNullWhen(false)] out ModelInfo model)
        {
            model = indexByName.Values.FirstOrDefault(m => string.Equals(m.Path, path, StringComparison.OrdinalIgnoreCase));
            if (model == null)
            {
                var odol = ReadModelInfoOnly(path);
                if (odol == null)
                {
                    return false;
                }
                var name = UniqueName(Path.GetFileNameWithoutExtension(path));
                model = new ModelInfo(name, path, odol.BoundingCenter.Vector3);
                indexByName.Add(name, model);
            }
            return true;
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

        private T Read<T>(Stream stream, string path) where T : IReadObject, new()
        {
            try
            {
                return StreamHelper.Read<T>(stream);
            }
            catch (Exception ex)
            {
                var physicalPath = fileSystem.GetLocationInfoForError(path) ?? path;
                throw new ApplicationException($"Unable to read file '{physicalPath}': {ex.Message}", ex);
            }
        }

        public BIS.P3D.ODOL.ModelInfo? ReadModelInfoOnly(string path)
        {
            using (var stream = fileSystem.OpenFileIfExists(path))
            {
                if (stream != null)
                {
                    var result = Read<P3DInfosOnly>(stream, path).ModelInfo as BIS.P3D.ODOL.ModelInfo;
                    if (result != null)
                    {
                        return result;
                    }
                    // Mikero Tools binarize into project drive temp, binarized file might be there
                    using (var streamTemp = fileSystem.OpenFileIfExists("temp\\" + path))
                    {
                        if (streamTemp != null)
                        {
                            return Read<P3DInfosOnly>(streamTemp, "temp\\" + path).ModelInfo as BIS.P3D.ODOL.ModelInfo;
                        }
                    }
                    // TODO: Binarize on the fly
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
                        return Read<ODOL>(stream, path);
                    }
                    // Mikero Tools binarize into project drive temp, binarized file might be there
                    using (var streamTemp = fileSystem.OpenFileIfExists("temp\\" + path))
                    {
                        if (streamTemp != null && P3D.IsODOL(streamTemp))
                        {
                            return Read<ODOL>(streamTemp, "temp\\" + path);
                        }
                    }
                    // TODO: Binarize on the fly
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
                            updatedModel = new ModelInfo(model.Name, model.Path, odol.BoundingCenter.Vector3);
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

        public async Task Load()
        {
            if (File.Exists(DefaultCachePath))
            {
                await LoadFrom(DefaultCachePath).ConfigureAwait(false);
            }
        }

        public async Task SaveTo(string path)
        {
            using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, indexByName.Values);
        }

        public Task Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DefaultCachePath)!);
            return SaveTo(DefaultCachePath);
        }

        public bool TryRegister(string name, string path)
        {
            var odol = ReadModelInfoOnly(path);
            if (odol == null)
            {
                return false;
            }

            var model = new ModelInfo(name, path, odol.BoundingCenter.Vector3);
            indexByName.Add(name, model);
            return true;
        }

        public bool? IsSlopeLandContact(string path)
        {
            if (!slopelandcontact.TryGetValue(path, out var isSlopeLandContact))
            {
                using (var file = fileSystem.OpenFileIfExists(path))
                {
                    if (file != null)
                    {
                        var infos = StreamHelper.Read<P3D>(file);
                        var placement = infos.LODs.FirstOrDefault(l => l.Resolution == 1E+13f)?.NamedProperties?.FirstOrDefault(n => n.Item1 == "placement")?.Item2;
                        isSlopeLandContact = !string.IsNullOrEmpty(placement) && placement.StartsWith("slope", StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        isSlopeLandContact = null;
                    }
                }
                slopelandcontact.Add(path, isSlopeLandContact);
            }
            return isSlopeLandContact;
        }

        public string? TryGetNoLandContact(string path)
        {                
            // Some models have a "_nolc" variant (No LandContact)
            if (path.EndsWith("_f.p3d", StringComparison.OrdinalIgnoreCase))
            {
                var altPath = path.Substring(0, path.Length - 6) + "_nolc_f.p3d";
                if (TryResolveByPath(altPath, out var model))
                {
                    return model.Path;
                }
            }
            else if (path.EndsWith(".p3d", StringComparison.OrdinalIgnoreCase))
            {
                var altPath = path.Substring(0, path.Length - 4) + "_nolc.p3d";
                if (TryResolveByPath(altPath, out var model))
                {
                    return model.Path;
                }
            }
            return null;
        }
    }
}
