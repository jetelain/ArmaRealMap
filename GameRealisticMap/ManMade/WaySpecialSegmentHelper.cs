using OsmSharp.Tags;

namespace GameRealisticMap.ManMade
{
    internal static class WaySpecialSegmentHelper
    {
        internal static WaySpecialSegment FromOSM(TagsCollectionBase tags)
        {
            if (tags.GetValue("embankment") == "yes")
            {
                return WaySpecialSegment.Embankment;
            }
            if (tags.GetValue("bridge") == "yes")
            {
                return WaySpecialSegment.Bridge;
            }
            return WaySpecialSegment.Normal;
        }
    }
}
