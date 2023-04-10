using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.Nature.Surfaces
{
    internal class SandSurfacesBuilder : BasicBuilderBase<SandSurfacesData>
    {
        public SandSurfacesBuilder(IProgressSystem progress)
            : base(progress)
        {

        }

        protected override SandSurfacesData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new SandSurfacesData(polygons);
        }

        protected override bool IsTargeted(TagsCollectionBase tags)
        {
            switch (tags.GetValue("natural"))
            {
                case "sand":
                case "beach":
                    return true;
            }
            return tags.GetValue("surface") == "sand";
        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return Enumerable.Empty<TerrainPolygon>();
        }
    }
}
