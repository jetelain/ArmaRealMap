namespace GameRealisticMap.Arma3.Edit
{
    public enum SlopeLandContactBehavior
    {
        /// <summary>
        /// Substitute if a "no land contact" version exists, otherwise tries to compensate according to the slope of the terrain.
        /// </summary>
        TryToCompensate,

        /// <summary>
        /// Substitute if a "no land contact" version exists, otherwise make object follow terrain (removes pitch/roll rotations as the game engine will do what is required).
        /// </summary>
        FollowTerrain,

        /// <summary>
        /// Assume that object have been placed ignoring the slope of the terrain (no pitch/roll within Eden Editor), keep land contact objects.
        /// </summary>
        Ignore
    }
}
