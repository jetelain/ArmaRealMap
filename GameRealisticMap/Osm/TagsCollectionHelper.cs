using System.Globalization;
using OsmSharp.Tags;

namespace GameRealisticMap.Osm
{
    public static class TagsCollectionHelper
    {
        public static float? GetDirection(this TagsCollectionBase tags)
        {
            if (tags.TryGetValue("direction", out var dir) && !string.IsNullOrEmpty(dir))
            {
                switch (dir)
                {
                    case "north":
                    case "N": return 0f;
                    case "NNE": return 22.5f;
                    case "NE": return 45f;
                    case "ENE": return 67.5f;
                    case "east":
                    case "E": return 90;
                    case "ESE": return 112.5f;
                    case "SE": return 135f;
                    case "SSE": return 157.5f;
                    case "south":
                    case "S": return 180;
                    case "SSW": return 202.5f;
                    case "SW": return 225;
                    case "WSW": return 247.5f;
                    case "west":
                    case "W": return 270;
                    case "WNW": return 292.5f;
                    case "NW": return 315;
                    case "NNW": return 337.5f;
                }
                float angle;
                if (float.TryParse(dir, NumberStyles.Any, CultureInfo.InvariantCulture, out angle))
                {
                    return angle;
                }
            }
            return null;
        }
    }
}
