using System.Numerics;

namespace GameRealisticMap.Geometries
{
    public static class VectorHelper
    {

        public static bool HasIntersection(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2, out Vector3 res)
        {
            var da = a2 - a1;
            var db = b2 - b1;
            var dc = b1 - a1;
            if (Vector3.Dot(dc, Vector3.Cross(da, db)) == 0.0)
            {
                var s = Vector3.Dot(Vector3.Cross(dc, db), Vector3.Cross(da, db)) / Vector3.Cross(da, db).LengthSquared();
                var t = Vector3.Dot(Vector3.Cross(dc, da), Vector3.Cross(da, db)) / Vector3.Cross(da, db).LengthSquared();
                if ((s >= 0 && s <= 1) && (t >= 0 && t <= 1))
                {
                    res = a1 + (da * s);
                    return true;
                }
            }
            res = default(Vector3);
            return false;
        }

        public static bool HasIntersection(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 res)
        {
            if (HasIntersection(new Vector3(a1, 0f), new Vector3(a2, 0f), new Vector3(b1, 0f), new Vector3(b2, 0f), out Vector3 res2))
            {
                res = new Vector2(res2.X, res2.Y);
                return true;
            }
            res = default(Vector2);
            return false;
        }

        public static float GetAngleFromYAxisInDegrees(Vector2 vector)
        {
            return (float)(Math.Atan2(-vector.X, vector.Y) * 180 / Math.PI);
        }

        public static float GetAngleFromXAxisInDegrees(Vector2 vector)
        {
            return (float)(Math.Atan2(vector.Y, vector.X) * 180 / Math.PI);
        }
    }
}
