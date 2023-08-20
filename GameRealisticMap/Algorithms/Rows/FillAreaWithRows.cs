using System.Numerics;
using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Algorithms.Following;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Algorithms.Rows
{
    public class FillAreaWithRows
    {
        public static void Fill<TModel>(Random random, IRowFillingDefinition<TModel> definition, List<PlacedModel<TModel>> layer, TerrainPolygon polygon)
        {
            var box = BoundingBox.Compute(polygon.Shell.ToArray());
            var v1 = box.Points[1].Vector - box.Points[0].Vector;
            var v2 = box.Points[3].Vector - box.Points[0].Vector;
            var rowSpacing = Vector2.Normalize(v2) * (float)definition.RowSpacing;
            var position = box.Points[0].Vector + (rowSpacing / 2);
            var done = definition.RowSpacing / 2;
            var max = v2.Length() - (definition.RowSpacing / 2);
            while (done < max)
            {
                var path = new TerrainPath( new TerrainPoint(position), new TerrainPoint(position + v1) );
                foreach (var realPath in path.ClippedBy(polygon))
                {
                    FollowPathWithObjects.PlaceOnPathNotFitted(random, definition, layer, realPath.Points);
                }
                position += rowSpacing;
                done += definition.RowSpacing;
            }
        }
    }
}
