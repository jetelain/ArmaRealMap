namespace ArmaRealMap.TerrainData.ElevationModel
{
    class ElevationConstraint
    {
        private readonly ElevationConstraintNode lowerThan;

        public ElevationConstraint(ElevationConstraintNode lowerThan)
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

        public ElevationConstraintNode LowerThan => lowerThan;

        internal float Apply(float elevation)
        {
            if (lowerThan != null)
            {
                var shouldBeLowerThan = lowerThan.Elevation.Value;
                if (elevation > shouldBeLowerThan)
                {
                    elevation = shouldBeLowerThan;
                }
            }
            return elevation;
        }
    }
}
