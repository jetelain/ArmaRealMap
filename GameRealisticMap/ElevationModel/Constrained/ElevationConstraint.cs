namespace GameRealisticMap.ElevationModel.Constrained
{
    internal sealed class ElevationConstraint
    {
        private readonly ElevationConstraintNode lowerThan;

        public ElevationConstraint(ElevationConstraintNode lowerThan)
        {
            this.lowerThan = lowerThan;
        }

        internal bool IsSolved
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

        internal ElevationConstraintNode LowerThan => lowerThan;

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
