using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.Geometries;
using MathNet.Numerics;
using MathNet.Numerics.LinearRegression;

namespace ArmaRealMap.TerrainData.ElevationModel
{
    internal class ElevationConstraintGrid : SimpleSpacialIndex<ElevationConstraintNode>
    {
        private readonly ElevationGrid initial;
        private readonly float planarInitialDela;
        private readonly float flatSegment;
        private readonly Vector2 MergeRadius = new Vector2(0.1f, 0.1f);

        private static readonly Matrix3x2 rotateP90 = Matrix3x2.CreateRotation(1.570796f);
        private static readonly Matrix3x2 rotateM90 = Matrix3x2.CreateRotation(-1.570796f);

        private readonly List<ElevationSmoothSegment> smoothSegments = new List<ElevationSmoothSegment>();

        public ElevationConstraintGrid(MapInfos map, ElevationGrid initial)
            : base(map.P1.Vector, new Vector2(map.Width, map.Height), 512)
        {
            this.initial = initial;
            this.planarInitialDela = map.CellSize / 2f;
            this.flatSegment = map.CellSize / 2f;
        }

        public ElevationConstraintNode Node(TerrainPoint point)
        {
            var existingList =  Search(point.Vector - MergeRadius, point.Vector + MergeRadius);
            if (existingList.Count > 0 )
            {
                return existingList.OrderBy(p => (p.Point.Vector - point.Vector).LengthSquared()).First();
            }
            var node = new ElevationConstraintNode(point, initial);
            Insert(point.Vector, point.Vector, node);
            return node;
        }

        public ElevationSmoothSegment CreateSmoothSegment(ElevationConstraintNode start, float sampling)
        {
            var segment = new ElevationSmoothSegment(start, sampling);
            smoothSegments.Add(segment);
            return segment;
        }

        public ElevationConstraintNode AddFlatSegment(TerrainPoint point, Vector2 normalVector, float width)
        {
            return AddFlatSegment(Node(point), normalVector, width);
        }

        public ElevationConstraintNode AddFlatSegment(ElevationConstraintNode node, Vector2 normalVector, float width)
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
                AddFlatSegment(node, seg, normalized);
                seg += flatSegment;
            }
            AddFlatSegment(node, radius, normalized);
            return node;
        }

        private void AddFlatSegment(ElevationConstraintNode node, float radius, Vector2 normalized)
        {
            Node(node.Point + (Vector2.Transform(normalized, rotateP90) * radius)).MustBeSameThan(node);
            Node(node.Point + (Vector2.Transform(normalized, rotateM90) * radius)).MustBeSameThan(node);
        }

        public void Solve()
        {
            var report = new ProgressReport("Solve", Count);

            int attempts = 0;
            while(Values.Any(v => !v.IsSolved) && attempts < 1000)
            {
                foreach(var value in Values.Where(v => v.CanSolve).ToList())
                {
                    value.Solve();
                    report.ReportOneDone();
                }
                attempts++;
            }

            int giveup = 0;
            foreach(var value in Values.Where(v => !v.IsSolved))
            {
                value.GiveUp();
                giveup++;
            }
            report.TaskDone();
        }

        public void Smooth()
        {
            var report = new ProgressReport("Smooth", smoothSegments.Count);
            foreach (var smooth in smoothSegments)
            {
                smooth.Apply();
                report.ReportOneDone();
            }
            report.TaskDone();
        }

        public void SolveAndApplyOnGrid()
        {
            Solve();

            Smooth();

            ApplyOnGrid();
        }


        public void ApplyOnGrid()
        {
            var unit = new Vector2(initial.area.CellSize);
            var report = new ProgressReport("ScanGrid", initial.area.Size);
            int done = 0;
            int changes = 0;
            var list = new ConcurrentBag<(int,int,Vector2,List<ElevationConstraintNode>)>();

            Parallel.For(0, initial.area.Size, y =>
            {
                for (int x = 0; x < initial.area.Size; x++)
                {
                    var point = new Vector2(x * initial.area.CellSize, y * initial.area.CellSize);
                    var constraints = Search(point - unit, point + unit).Where(c => c.IsSolved).ToList();
                    if (constraints.Count > 0 )
                    {
                        list.Add(new (x, y, point, constraints));
                    }
                }
                report.ReportItemsDone(Interlocked.Increment(ref done));
            });
            report.TaskDone();

            report = new ProgressReport("ApplyGrid", list.Count * 20);
            for (var i = 0; i < 20; ++i)
            {
                changes = 0;
                foreach (var (x, y, point, constraints) in list)
                {
                    var intialElevation = initial.elevationGrid[x, y];
                    var elevation = Estimate(point, constraints, intialElevation);
                    if (Math.Abs(elevation - intialElevation) > 0.05f)
                    {
                        changes++;
                    }
                    initial.elevationGrid[x, y] = elevation;
                    report.ReportOneDone();
                }
                Trace.WriteLine($"{changes} changes on elevation grid");
            }
            report.TaskDone();
            Trace.Flush();

        }

        private float Estimate(Vector2 point, List<ElevationConstraintNode> constraints, float initialElevation)
        {
            if (constraints.Count == 1)
            {
                return constraints[0].Elevation.Value;
            }
            //if (constraints.Count == 2)
            //{
            //    return LineResolution(point, constraints);
            //}
            //var a = PlaneResolution(point, constraints, initialElevation, 4); ;
            //var b = PlaneResolution(point, constraints, initialElevation, 1); ;
            //var c = constraints.Select(c => c.Elevation.Value).Average();
            //Trace.WriteLine($"{a} <> {b} <> {c}");

            return PlaneResolution(point, constraints, initialElevation, 4);
        }

        private float PlaneResolution(Vector2 point, List<ElevationConstraintNode> constraints, float? initialElevation, int weight)
        {
            var data = constraints.Select(c => new Tuple<float[], float>(new float[] {
                            c.Point.X - point.X,
                            c.Point.Y - point.Y}, c.Elevation.Value));

            if (initialElevation != null)
            {
                var planarInitial = new[] {
                    new Tuple<float[], float>( new[] { -planarInitialDela, 0f}, initialElevation.Value),
                    new Tuple<float[], float>( new[] { +planarInitialDela, 0f}, initialElevation.Value),
                    new Tuple<float[], float>( new[] { 0f, -planarInitialDela}, initialElevation.Value),
                    new Tuple<float[], float>( new[] { 0f, +planarInitialDela}, initialElevation.Value),
                };

                data = data.SelectMany(e => Enumerable.Repeat(e, weight)).Concat(planarInitial);
            }

                //try
                //{
            var result = MultipleRegression.NormalEquations(data, true);

            if (MathF.Abs(result[1]) >= 1.5f || MathF.Abs(result[2]) >= 1.5f) // Too important >150%
            {
                return constraints.Select(c => c.Elevation.Value).Average();
            }
            return result[0];
            //}
            //catch (ArgumentException)
            //{
            //    return LineResolution(point, constraints);
            //}
        }

        //private float LineResolution(Vector2 point, List<ElevationConstraintNode> constraints)
        //{
        //    float[] result;
        //    var data = constraints.Select(c => new Tuple<float[], float>(new float[] { c.Point.X - point.X  }, c.Elevation.Value)).ToList();
        //    try 
        //    {
        //        result = MultipleRegression.NormalEquations(data, true);
        //    }
        //    catch (ArgumentException)
        //    {
        //        data = constraints.Select(c => new Tuple<float[], float>(new float[] { c.Point.Y - point.Y  }, c.Elevation.Value)).ToList();
        //        result = MultipleRegression.NormalEquations(data, true);
        //    }
        //    if (MathF.Abs(result[1]) >= 1f) // Too important
        //    {
        //        return constraints.Select(c => c.Elevation.Value).Average();
        //    }
        //    return result[0];
        //}
    }
}
