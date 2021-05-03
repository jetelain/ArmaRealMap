using ArmaRealMap.GroundTextureDetails;
using SixLabors.ImageSharp;

namespace ArmaRealMap
{
    class AreaCategory
    {
        internal readonly TerrainMaterial TerrainMaterial;
        internal Color GroundTextureColorCode => TerrainMaterial.Color;
        internal readonly int GroundTexturePriority;
        internal readonly ObjectCategory? BuildingType;

        public AreaCategory Parent { get; }

        public AreaCategory(TerrainMaterial material, int priority, ObjectCategory? buildingCategory = null)
        {
            TerrainMaterial = material;
            GroundTexturePriority = priority;
            BuildingType = buildingCategory;
        }

        public AreaCategory(AreaCategory parent, ObjectCategory? buildingCategory)
        {
            TerrainMaterial = parent.TerrainMaterial;
            GroundTexturePriority = parent.GroundTexturePriority;
            BuildingType = buildingCategory;
            Parent = parent;
        }
        public AreaCategory(AreaCategory parent, int priority, ObjectCategory? buildingCategory)
        {
            TerrainMaterial = parent.TerrainMaterial;
            GroundTexturePriority = priority;
            BuildingType = buildingCategory;
            Parent = parent;
        }

        internal static readonly AreaCategory Water = new AreaCategory(TerrainMaterial.WetLand, 1);
        internal static readonly AreaCategory Forest = new AreaCategory(TerrainMaterial.Forest, 4);
        internal static readonly AreaCategory WetLand = new AreaCategory(TerrainMaterial.WetLand, 2);
        internal static readonly AreaCategory Grass = new AreaCategory(TerrainMaterial.GrassShort, 3);
        internal static readonly AreaCategory FarmLand = new AreaCategory(TerrainMaterial.FarmLand, 5);
        internal static readonly AreaCategory Sand = new AreaCategory(TerrainMaterial.Sand, 6);
        internal static readonly AreaCategory Rocks = new AreaCategory(TerrainMaterial.Rock, 7);
        internal static readonly AreaCategory Concrete = new AreaCategory(TerrainMaterial.Concrete, 9);
        internal static readonly AreaCategory Dirt = new AreaCategory(TerrainMaterial.Dirt, 8);
        internal static readonly AreaCategory Military = new AreaCategory(Grass, 10, ObjectCategory.Military);
        internal static readonly AreaCategory Residential = new AreaCategory(Grass, ObjectCategory.Residential);
        internal static readonly AreaCategory Industrial = new AreaCategory(Grass, ObjectCategory.Industrial);
        internal static readonly AreaCategory Retail = new AreaCategory(Grass, ObjectCategory.Retail);

        internal static readonly AreaCategory[] BuildingCategorizers = new[] { Residential, Industrial, Retail, Military };

        internal static readonly AreaCategory Building = new AreaCategory(TerrainMaterial.Dirt, 0);
        internal static readonly AreaCategory BuildingHistoricalFort = new AreaCategory(Grass, ObjectCategory.HistoricalFort);
        internal static readonly AreaCategory BuildingRetail = new AreaCategory(Building, ObjectCategory.Retail);
        internal static readonly AreaCategory BuildingChurch = new AreaCategory(Building, ObjectCategory.Church);

        internal static readonly AreaCategory Road = new AreaCategory(TerrainMaterial.Dirt, 0);

        public bool IsBuilding
        {
            get
            {
                return this == Building || Parent == Building;
            }
        }
    }
}
