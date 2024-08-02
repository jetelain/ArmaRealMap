using GameRealisticMap.Geometries;

namespace GameRealisticMap.Nature.Watercourses
{
    internal class WatercourseRadialBuilder : BasicRadialBuilder<WatercourseRadialData, WatercoursesData>
    {
        public WatercourseRadialBuilder()
            : base(WatercourseRadialData.Width)
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
