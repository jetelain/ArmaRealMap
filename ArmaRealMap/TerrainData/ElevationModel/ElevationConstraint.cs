namespace ArmaRealMap.TerrainData.ElevationModel
{
    class ElevationConstraint
    {
        private readonly ElevationConstraintNode lowerThan;
        private readonly float lowerShift;

        public ElevationConstraint(ElevationConstraintNode lowerThan = null, float lowerShift = 0f, bool optional = false)
        {
            this.lowerThan = lowerThan;
            this.lowerShift = lowerShift;
            this.Optional = optional;
        }

        public ElevationConstraintNode Node => lowerThan;

        public bool Optional { get; }

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
            if (lowerThan != null)
            {
                var minimalElevation = lowerThan.Elevation.Value - lowerShift;
                if (elevation > minimalElevation)
                {
                    elevation = minimalElevation;
                }
            }
            return elevation;
        }
    }
}
