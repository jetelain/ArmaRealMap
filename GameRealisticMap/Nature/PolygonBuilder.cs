using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature
{
    internal class PolygonBuilder : PolygonBuilderBase
    {
        private readonly Func<TagsCollectionBase, bool> isTargeted;
        private readonly IEnumerable<TerrainPolygon> priority;

        public PolygonBuilder(IProgressSystem progress, Func<TagsCollectionBase, bool> isTargeted, IEnumerable<TerrainPolygon> priority)
            : base(progress)
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
