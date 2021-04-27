using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;

namespace ArmaRealMap
{
    class Category
    {
        internal readonly Color GroundTextureColorCode;
        internal readonly int GroundTexturePriority;
        internal readonly BuildingCategory? BuildingType;

        public Category Parent { get; }

        public Category(Color color, int priority, BuildingCategory? buildingCategory = null)
        {
            GroundTextureColorCode = color;
            GroundTexturePriority = priority;
            BuildingType = buildingCategory;
        }

        public Category(Category parent, BuildingCategory? buildingCategory)
        {
            GroundTextureColorCode = parent.GroundTextureColorCode;
            GroundTexturePriority = parent.GroundTexturePriority;
            BuildingType = buildingCategory;
            Parent = parent;
        }
        public Category(Category parent, int priority, BuildingCategory? buildingCategory)
        {
            GroundTextureColorCode = parent.GroundTextureColorCode;
            GroundTexturePriority = priority;
            BuildingType = buildingCategory;
            Parent = parent;
        }

        internal static readonly Category Water = new Category(Color.Blue, 1);
        internal static readonly Category Forest = new Category(Color.Green, 3);
        internal static readonly Category WetLand = new Category(Color.LightBlue, 2);
        internal static readonly Category Grass = new Category(Color.YellowGreen, 4);
        internal static readonly Category FarmLand = new Category(Color.Yellow, 5);
        internal static readonly Category Sand = new Category(Color.SandyBrown, 6);
        internal static readonly Category Rocks = new Category(Color.DarkGray, 7);
        internal static readonly Category Concrete = new Category(Color.Gray, 8);
        internal static readonly Category Military = new Category(Grass, 9, BuildingCategory.Military);
        internal static readonly Category Residential = new Category(Concrete, BuildingCategory.Residential);
        internal static readonly Category Industrial = new Category(Concrete, BuildingCategory.Industrial);
        internal static readonly Category Retail = new Category(Concrete, BuildingCategory.Retail);

        internal static readonly Category[] BuildingCategorizers = new[] { Residential, Industrial, Retail, Military };

        internal static readonly Category Building = new Category(Color.Black, 0);
        internal static readonly Category BuildingHistoricalFort = new Category(Building, BuildingCategory.HistoricalFort);
        internal static readonly Category BuildingRetail = new Category(Building, BuildingCategory.Retail);
        internal static readonly Category BuildingChurch = new Category(Building, BuildingCategory.Church);

        public bool IsBuilding
        {
            get
            {
                return this == Building || Parent == Building;
            }
        }
    }
}
