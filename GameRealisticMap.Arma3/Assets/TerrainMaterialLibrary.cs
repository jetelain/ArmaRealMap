using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;
using GameRealisticMap.Arma3.GameEngine.Materials;
using SixLabors.ImageSharp.PixelFormats;

namespace GameRealisticMap.Arma3.Assets
{
    public class TerrainMaterialLibrary
    {
        private readonly Dictionary<Rgb24, TerrainMaterial> indexByColor = new Dictionary<Rgb24, TerrainMaterial>();
        private readonly Dictionary<TerrainMaterialUsage, TerrainMaterial> indexByUsage = new Dictionary<TerrainMaterialUsage, TerrainMaterial>();
        private readonly List<TerrainMaterialDefinition> definitions = new List<TerrainMaterialDefinition>();

        public const double DefaultTextureSizeInMeters = 4;

        public TerrainMaterialLibrary()
        {
            var none = new TerrainMaterial("", "", new Rgb24(), null);
            indexByColor[none.Id] = none;
            foreach(var id in Enum.GetValues<TerrainMaterialUsage>())
            {
                indexByUsage[id] = none;
            }
        }

        [JsonConstructor]
        public TerrainMaterialLibrary(List<TerrainMaterialDefinition> definitions, double textureSizeInMeters = DefaultTextureSizeInMeters)
        {
            this.definitions.AddRange(definitions);

            foreach (var definition in definitions)
            {
                indexByColor.Add(definition.Material.Id, definition.Material);

                foreach (var usage in definition.Usages)
                {
                    // tolerate duplicates
                    indexByUsage[usage] = definition.Material;
                }
            }
            TextureSizeInMeters = textureSizeInMeters;
        }

        public double TextureSizeInMeters { get; } = DefaultTextureSizeInMeters;

        public TerrainMaterial GetMaterialById(Rgb24 id)
        {
            if (indexByColor.TryGetValue(id, out var material))
            {
                return material;
            }
            var vector = id.ToScaledVector4();
            return definitions.OrderByDescending(d => Vector4.DistanceSquared(vector, d.Material.Id.ToScaledVector4())).First().Material;
        }

        public TerrainMaterial GetMaterialByUsage(TerrainMaterialUsage usage)
        {
            if (indexByUsage.TryGetValue(usage, out var material))
            {
                return material;
            }
            if (usage == TerrainMaterialUsage.Default)
            {
                return indexByColor.Values.First();
            }
            if (usage == TerrainMaterialUsage.ScreeSurface)
            {
                return GetMaterialByUsage(TerrainMaterialUsage.RockGround);
            }
            return GetMaterialByUsage(TerrainMaterialUsage.Default);
        }

        public List<TerrainMaterialDefinition> Definitions => definitions;

        public SurfaceConfig? GetSurface(TerrainMaterial m)
        {
            return definitions.FirstOrDefault(d => d.Material == m)?.Surface;
        }

        [JsonIgnore]
        public IEnumerable<SurfaceConfig> Surfaces => definitions.Where(d => d.Surface != null).Select(d => d.Surface!);
    }
}