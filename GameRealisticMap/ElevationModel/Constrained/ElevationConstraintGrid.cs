using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using MathNet.Numerics.LinearRegression;

namespace GameRealisticMap.ElevationModel.Constrained
{
    public class ElevationConstraintGrid : SimpleSpacialIndex<ElevationConstraintNode>
    {
        private readonly ITerrainArea area;
        private readonly ElevationGrid initial;
        private readonly IProgressSystem progress;
        private readonly float planarInitialDela;
        private readonly float flatSegment;
        private readonly Vector2 MergeRadius = new Vector2(0.1f, 0.1f);

        private static readonly Matrix3x2 rotateP90 = Matrix3x2.CreateRotation(1.570796f);
        private static readonly Matrix3x2 rotateM90 = Matrix3x2.CreateRotation(-1.570796f);

        private readonly List<ElevationSmoothSegment> smoothSegments = new List<ElevationSmoothSegment>();

        public ElevationConstraintGrid(ITerrainArea area, ElevationGrid initial, IProgressSystem progress)
            : base(Vector2.Zero, new Vector2(area.SizeInMeters), 512)
        {
            this.area = area;
            this.initial = initial;
            this.progress = progress;
            planarInitialDela = area.GridCellSize / 2f;
            flatSegment = area.GridCellSize / 2f;
        }

        public ElevationGrid Grid => initial;

        public ElevationConstraintNode NodeHard(TerrainPoint point)
        {
            return Node(point, false);
        }

        public ElevationConstraintNode NodeSoft(TerrainPoint point)
        {
            return Node(point, true);
        }

        private ElevationConstraintNode Node(TerrainPoint point, bool isSoft)
        {
            var existingList = Search(point.Vector - MergeRadius, point.Vector + MergeRadius);
            if (existingList.Count > 0)
            {
                var enode = existingList.OrderBy(p => (p.Point.Vector - point.Vector).LengthSquared()).First();
                enode.IsSoft = enode.IsSoft && isSoft;
                return enode;
            }
            var node = new ElevationConstraintNode(point, initial);
            node.IsSoft = isSoft;
            Insert(point.Vector, point.Vector, node);
            return node;
        }

        public ElevationSmoothSegment CreateSmoothSegment(ElevationConstraintNode start, float sampling)
        {
            var segment = new ElevationSmoothSegment(start, sampling);
            smoothSegments.Add(segment);
            return segment;
        }

        //public ElevationConstraintNode AddFlatSegment(TerrainPoint point, Vector2 normalVector, float width)
        //{
        //    return AddFlatSegment(NodeHard(point), normalVector, width);
        //}

        public ElevationConstraintNode AddFlatSegmentHard(ElevationConstraintNode node, Vector2 normalVector, float width)
        {
            if (normalVector == Vector2.Zero)
            {
                return node;
            }
            var radius = width / 2f;
            var normalized = Vector2.Normalize(normalVector);
            var seg = flatSegment;
            while (radius > seg)
            {
                AddFlatSegmentHard(node, seg, normalized);
                seg += flatSegment;
            }
            AddFlatSegmentHard(node, radius, normalized);
            return node;
        }

        private void AddFlatSegmentHard(ElevationConstraintNode node, float radius, Vector2 normalized)
        {
            NodeHard(node.Point + Vector2.Transform(normalized, rotateP90) * radius).MustBeSameThan(node);
            NodeHard(node.Point + Vector2.Transform(normalized, rotateM90) * radius).MustBeSameThan(node);
        }
        /*
        public IEnumerable<ElevationConstraintNode> GetReference(ElevationConstraintNode node, Vector2 normalVector, float width)
        {
            if (normalVector == Vector2.Zero)
            {
                return new ElevationConstraintNode[0];
            }
            var radius = width / 2f;
            var normalized = Vector2.Normalize(normalVector);
            return new[] 
            {
                Node(node.Point + (Vector2.Transform(normalized, rotateP90) * radius), true),
                Node(node.Point + (Vector2.Transform(normalized, rotateM90) * radius), true)
            };
        }
        */
        private void Solve()
        {
            SolveLoop();

            var unsolved = Values.Where(v => !v.IsSolved).ToList();
            foreach (var value in unsolved)
            {
                value.GiveUp();
            }
        }

        private void SolveLoop()
        {
            using var report = progress.CreateStep("Solve", Count);

            bool changed = true;
            var remain = Values;
            while (remain.Any() && changed)
            {
                changed = false;
                var canSolve = remain.Where(v => v.CanSolve).ToList();
                foreach (var value in canSolve)
                {
                    value.Solve();
                    report.ReportOneDone();
                    changed = true;
                }
                remain = remain.Where(v => !v.IsSolved).ToList();
            }
        }

        private void Smooth()
        {
            using var report = progress.CreateStep("Smooth", smoothSegments.Count);
            foreach (var smooth in smoothSegments)
            {
                smooth.Apply();
                report.ReportOneDone();
            }
        }

        public void SolveAndApplyOnGrid()
        {
            Solve();

            Smooth();

            ApplyOnGrid();
        }

        private void ApplyOnGrid()
        {
            var unit = new Vector2(area.GridCellSize);
            var two = new Vector2(area.GridCellSize * 2);
            var report = progress.CreateStep("ScanGrid", area.GridSize);
            int done = 0;
            int changes = 0;
            var listHard = new ConcurrentBag<(int, int, Vector2, List<ElevationConstraintNode>)>();
            var listSoft = new ConcurrentBag<(int, int, Vector2, List<ElevationConstraintNode>)>();

            Parallel.For(0, area.GridSize, y =>
            {
                for (int x = 0; x < area.GridSize; x++)
                {
                    var point = new Vector2(x * area.GridCellSize, y * area.GridCellSize);
                    var hard = Search(point - unit, point + unit).Where(c => c.IsSolved).ToList();
                    if (hard.Count > 0)
                    {
                        listHard.Add(new(x, y, point, hard));
                    }
                    else
                    {
                        var soft = Search(point - two, point + two).Where(c => c.IsSolved && c.IsSoft).ToList();
                        if (soft.Count > 0)
                        {
                            listSoft.Add(new(x, y, point, soft));
                        }
                    }
                }
                report.Report(Interlocked.Increment(ref done));
            });
            report.Dispose();

            report = progress.CreateStep("ApplyGrid", (listHard.Count + listSoft.Count) * 20);
            for (var i = 0; i < 20; ++i)
            {
                changes = 0;
                foreach (var (x, y, point, constraints) in listHard)
                {
                    var intialElevation = initial[x, y];
                    var elevation = Estimate(point, constraints, intialElevation);
                    if (Math.Abs(elevation - intialElevation) > 0.05f)
                    {
                        changes++;
                    }
                    initial[x, y] = elevation;
                    report.ReportOneDone();
                }
                // Now that all hard points have been applied, try to smooth around soft points
                foreach (var (x, y, point, constraints) in listSoft)
                {
                    var intialElevation = initial[x, y];
                    var elevation = SoftAround(intialElevation, point, initial, constraints);
                    if (Math.Abs(elevation - intialElevation) > 0.05f)
                    {
                        changes++;
                    }
                    initial[x, y] = elevation;
                    report.ReportOneDone();
                }
                Trace.WriteLine($"{changes} changes on elevation grid");
            }
            report.Dispose();
            Trace.Flush();
        }

        private float SoftAround(float initialElevation, Vector2 point, ElevationGrid grid, List<ElevationConstraintNode> constraints)
        {
            //  *   |   *
            //      |
            // -----*------
            //      |  
            //  *   |   *
            var elevation = new List<float>() {
                initialElevation,
                grid.ElevationAt(new TerrainPoint(point.X + planarInitialDela, point.Y + planarInitialDela)),
                grid.ElevationAt(new TerrainPoint(point.X + planarInitialDela, point.Y - planarInitialDela)),
                grid.ElevationAt(new TerrainPoint(point.X - planarInitialDela, point.Y - planarInitialDela)),
                grid.ElevationAt(new TerrainPoint(point.X - planarInitialDela, point.Y + planarInitialDela))
            }.Average();
            var protect = constraints.Where(c => c.IsProtected).Min(c => c.LowerLimitAbsoluteElevation);
            if (protect != null)
            {
                if (initialElevation <= protect.Value)
                {
                    return initialElevation; // below protection, likely within the lake, do not touch !
                }
                if (elevation < protect.Value)
                {
                    return protect.Value;
                }
            }
            return elevation;
        }

        private float Estimate(Vector2 point, List<ElevationConstraintNode> constraints, float initialElevation)
        {
            if (constraints.Count == 1)
            {
                return constraints[0].Elevation.Value;
            }
            return PlaneResolution(point, constraints, initialElevation, 4);
        }

        private float PlaneResolution(Vector2 point, List<ElevationConstraintNode> constraints, float? initialElevation, int weight)
        {
            var data = constraints.Select(c => new Tuple<float[], float>(new float[] {
                            c.Point.X - point.X,
                            c.Point.Y - point.Y}, c.Elevation.Value));

            if (initialElevation != null)
            {
                //      *   
                //      |
                // --*--+--*---
                //      |  
                //      *   
                var planarInitial = new[] {
                    new Tuple<float[], float>( new[] { -planarInitialDela, 0f}, initialElevation.Value),
                    new Tuple<float[], float>( new[] { +planarInitialDela, 0f}, initialElevation.Value),
                    new Tuple<float[], float>( new[] { 0f, -planarInitialDela}, initialElevation.Value),
                    new Tuple<float[], float>( new[] { 0f, +planarInitialDela}, initialElevation.Value),
                };
                data = data.SelectMany(e => Enumerable.Repeat(e, weight)).Concat(planarInitial);
            }

            var result = MultipleRegression.NormalEquations(data, true);
            if (MathF.Abs(result[1]) >= 1.5f || MathF.Abs(result[2]) >= 1.5f) // Too important >150%
            {
                return constraints.Select(c => c.Elevation.Value).Average();
            }
            return result[0];
        }
    }
}
