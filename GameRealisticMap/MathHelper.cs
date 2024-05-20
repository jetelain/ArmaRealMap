namespace GameRealisticMap
{
    public static class MathHelper
    {
        public static float ToRadians(float angle)
        {
            if (angle == 0)
            {
                return 0;
            }
            return angle * MathF.PI / 180f;
        }

        public static float FromRadians(double angle)
        {
            if (angle == 0)
            {
                return 0;
            }
            return (float)(angle * 180.0 / Math.PI);
        }

    }
}
