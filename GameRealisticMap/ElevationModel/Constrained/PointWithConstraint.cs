using System.Numerics;

namespace GameRealisticMap.ElevationModel.Constrained
{
    internal class PointWithConstraint
    {
        public PointWithConstraint(int x, int y, Vector2 point, List<ElevationConstraintNode> constraints)
        {
            X = x;
            Y = y;
            Point = point;
            Constraints = constraints;
        }

        public int X { get; }
        public int Y { get; }
        public Vector2 Point { get; }
        public List<ElevationConstraintNode> Constraints { get; }
    }
}