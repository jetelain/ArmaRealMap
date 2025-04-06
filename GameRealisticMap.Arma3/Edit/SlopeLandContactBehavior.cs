namespace GameRealisticMap.Arma3.Edit
{
    public enum SlopeLandContactBehavior
    {
        /// <summary>
        /// Tries to compensate according to the slope of the terrain.
        /// </summary>
        TryToCompensate,

        /// <summary>
        /// Assume that object have to follow terrain : Removes pitch/roll rotations as the game engine will make the object to follow the terrain.
        /// </summary>
        FollowTerrain,

        /// <summary>
        /// Assume that object have been placed ignoring the slope of the terrain (no pitch/roll within Eden Editor, in that case FollowTerrain should gives the same result)
        /// </summary>
        Ignore
    }
}
