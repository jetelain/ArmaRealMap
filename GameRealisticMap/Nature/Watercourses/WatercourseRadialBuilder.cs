using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Nature.Watercourses
{
    internal class WatercourseRadialBuilder : BasicRadialBuilder<WatercourseRadialData, WatercoursesData>
    {
        public WatercourseRadialBuilder(IProgressSystem progress)
            : base(progress, WatercourseRadialData.Width)
        {

        }

        protected override IEnumerable<TerrainPolygon> GetPriority(IBuildContext context)
        {
            return base.GetPriority(context);
        }

        protected override WatercourseRadialData CreateWrapper(List<TerrainPolygon> polygons)
        {
            return new WatercourseRadialData(polygons);
        }
    }
}
