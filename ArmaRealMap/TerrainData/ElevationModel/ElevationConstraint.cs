namespace ArmaRealMap.TerrainData.ElevationModel
{
    class ElevationConstraint
    {
        private readonly ElevationConstraintNode lowerThan;

        public ElevationConstraint(ElevationConstraintNode lowerThan = null)
        {
            this.lowerThan = lowerThan;
        }

        public bool IsSolved
        {
            get
            {
                if (lowerThan != null && !lowerThan.IsSolved)
                {
                    return false;
                }
                return true;
            }
        }

        internal float Apply(float elevation)
        {
            if (lowerThan != null && lowerThan.Elevation.Value > elevation)
            {
                elevation = lowerThan.Elevation.Value;
            }
            return elevation;
        }
    }
}
