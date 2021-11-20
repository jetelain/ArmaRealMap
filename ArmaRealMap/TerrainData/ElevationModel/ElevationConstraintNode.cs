using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Geometries;

namespace ArmaRealMap.TerrainData.ElevationModel
{
    class ElevationConstraintNode
    {
        public ElevationConstraintNode (TerrainPoint point, ElevationGrid initial)
             : this(point, initial.ElevationAt(point))
        {

        }

        public ElevationConstraintNode(TerrainPoint point, float elevation)
        {
            Point = point;
            InitialElevation = elevation;
        }

        public TerrainPoint Point { get; }

        public float InitialElevation { get; }

        public float? WantedInitialShift { get; set; }

        public float? Elevation { get; private set; }

        public HashSet<ElevationConstraintNode> SameThan { get; } = new HashSet<ElevationConstraintNode>();

        public List<ElevationConstraint> Constraints { get; } = new List<ElevationConstraint>();

        public ElevationConstraintNode PinToInitial()
        {
            if (SameThan.Count > 0)
            {
                SetElevation(new[] { InitialElevation}.Concat(SameThan.Select(e => e.InitialElevation)).Average());
            }
            else
            {
                SetElevation(InitialElevation);
            }
            return this;
        }

        public void MustBeSameThan(ElevationConstraintNode other)
        {
            other.SameThan.Add(this);
            SameThan.Add(other);
            if(Elevation != null)
            {
                other.SetElevation(Elevation.Value);
            }
            if (other.Elevation != null)
            {
                SetElevation(other.Elevation.Value);
            }
        }

        public void MustBeLowerThan(ElevationConstraintNode other)
        {
            Constraints.Add(new ElevationConstraint(lowerThan: other));
        }

        public bool CanSolve
        {
            get { return Constraints.Concat(SameThan.SelectMany(n => n.Constraints)).All(l => l.IsSolved); }
        }

        public bool IsSolved
        {
            get { return Elevation.HasValue; }
        }

        public void Solve()
        {
            if (IsSolved)
            {
                return;
            }
            SetElevationWidthConstraint(ComputeBaseElevation());
        }

        public void SetElevation(float elevation)
        {
            Elevation = elevation;
            foreach (var other in SameThan)
            {
                other.Elevation = elevation;
            }
        }

        public void SetElevationWidthConstraint(float elevation)
        {
            foreach (var constraint in Constraints.Concat(SameThan.SelectMany(n => n.Constraints)))
            {
                elevation = constraint.Apply(elevation);
            }

            Elevation = elevation;
            foreach (var other in SameThan)
            {
                other.Elevation = elevation;
            }
        }

        private float ComputeBaseElevation()
        {
            var elevation = new[] { InitialElevation }.Concat(SameThan.Select(e => e.InitialElevation)).Average();
            var shift = new[] { WantedInitialShift }.Concat(SameThan.Select(e => e.WantedInitialShift)).Where(w => w.HasValue).Select(w => w.Value).ToList();
            if (shift.Any())
            {
                elevation += shift.Average();
            }
            return elevation;
        }

        internal void GiveUp()
        {
            SetElevation(ComputeBaseElevation());
        }
    }
}
