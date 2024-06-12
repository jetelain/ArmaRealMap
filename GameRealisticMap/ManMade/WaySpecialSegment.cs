namespace GameRealisticMap.ManMade
{
    public enum WaySpecialSegment
    {
        Normal,
        Embankment,
        Bridge,
        Crossing, // For railway only

        /// <summary>
        /// Service road for private service.
        /// 
        /// Might be ignored if too small (household driveway)
        /// </summary>
        PrivateService
    }
}