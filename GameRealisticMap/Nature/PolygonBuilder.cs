using GameRealisticMap.Geometries;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature
{
    internal class PolygonBuilder : PolygonBuilderBase
    {
        private readonly Func<TagsCollectionBase, bool> isTargeted;
        private readonly IEnumerable<TerrainPolygon> priority;

        public PolygonBuilder(Func<TagsCollectionBase, bool> isTargeted, IEnumerable<TerrainPolygon> priority)
        {
            this.isTargeted = isTargeted;
            this.priority = priority;
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return priority;
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            return isTargeted(tags);
        }
    }
}
