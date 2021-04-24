using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;

namespace ArmaRealMap
{
    class Category
    {
        internal readonly Color Color;
        internal readonly int Priority;

        public Category(Color color, int priority)
        {
            Color = color;
            Priority = priority;
        }

        internal static Category Water = new Category(Color.Blue, 1);
        internal static Category WetLand = new Category(Color.LightBlue, 2);
        internal static Category Forest = new Category(Color.Green, 3);
        internal static Category Grass = new Category(Color.YellowGreen, 4);
        internal static Category FarmLand = new Category(Color.Yellow, 5);
        internal static Category Sand = new Category(Color.SandyBrown, 6);
        internal static Category Rocks = new Category(Color.DarkGray, 7);
        internal static Category Concrete = new Category(Color.Gray, 8);

        internal static Category Building = new Category(Color.Black, 0);
    }
}
