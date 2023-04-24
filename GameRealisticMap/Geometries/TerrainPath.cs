﻿using System.Numerics;
using System.Text.Json.Serialization;
using ClipperLib;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Distance;

namespace GameRealisticMap.Geometries
{
    public class TerrainPath : ITerrainEnvelope, ITerrainGeo
    {
        private readonly Lazy<LineString> asLineString;

        public TerrainPath(params TerrainPoint[] points)
            : this(points.ToList())
        {
        }

        [JsonConstructor]
        public TerrainPath(List<TerrainPoint> points)
        {
            this.Points = points;
            MinPoint = new TerrainPoint(points.Min(p => p.X), points.Min(p => p.Y));
            MaxPoint = new TerrainPoint(points.Max(p => p.X), points.Max(p => p.Y));
            EnveloppeSize = MaxPoint.Vector - MinPoint.Vector;
            asLineString = new Lazy<LineString>(() => ToLineString(c => new Coordinate(c.X, c.Y)));
        }

        public List<TerrainPoint> Points { get; }

        [JsonIgnore]
        public TerrainPoint FirstPoint => Points[0];

        [JsonIgnore]
        public TerrainPoint LastPoint => Points[Points.Count-1];

        [JsonIgnore]
        public TerrainPoint MinPoint { get; }

        [JsonIgnore]
        public TerrainPoint MaxPoint { get; }

        [JsonIgnore]
        public Vector2 EnveloppeSize { get; }

        [JsonIgnore]
        public LineString AsLineString => asLineString.Value;

        [JsonIgnore]
        public float Length 
        { 
            get
            {
                var length = 0f;
                var prev = FirstPoint;
                TerrainPoint point;
                for(int i = 1; i < Points.Count; ++i)
                {
                    point = Points[i];
                    length += (point.Vector - prev.Vector).Length();
                    prev = point;
                }
                return length;
            }
        }

        public LineString ToLineString(Func<TerrainPoint, Coordinate> project)
        {
            return new LineString(Points.Select(project).ToArray());
        }

        public IEnumerable<TerrainPolygon> ToTerrainPolygon(float width)
        {
            return TerrainPolygon.FromPath(Points, width);
        }
        public static IEnumerable<TerrainPath> FromGeometry(IGeometry geometry, Func<Coordinate, TerrainPoint> project)
        {
            switch (geometry.OgcGeometryType)
            {
                case OgcGeometryType.LineString:
                    return new[] { new TerrainPath(((ILineString)geometry).Coordinates.Select(project).ToList()) };
            }
            return new TerrainPath[0];
        }

        public List<TerrainPath> Intersection(TerrainPolygon polygon)
        {
            var clipper = new Clipper();
            clipper.AddPath(Points.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, false);
            clipper.AddPath(polygon.Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true);
            foreach (var hole in polygon.Holes)
            {
                clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true); // EvenOdd will do the job
            }
            var result = new PolyTree();
            clipper.Execute(ClipType.ctIntersection, result);
            if (result.Childs.Any(c => c.ChildCount != 0))
            {
                throw new NotSupportedException();
            }
            return result.Childs.Select(c => new TerrainPath(c.Contour.Select(p => new TerrainPoint(p)).ToList())).ToList();
        }

        public List<TerrainPath> Substract(TerrainPolygon polygon)
        {
            var clipper = new Clipper();
            clipper.AddPath(Points.Select(c => c.ToIntPoint()).ToList(), PolyType.ptSubject, false);
            clipper.AddPath(polygon.Shell.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true);
            foreach (var hole in polygon.Holes)
            {
                clipper.AddPath(hole.Select(c => c.ToIntPoint()).ToList(), PolyType.ptClip, true); // EvenOdd will do the job
            }
            var result = new PolyTree();
            clipper.Execute(ClipType.ctDifference, result);
            if (result.Childs.Any(c => c.ChildCount != 0))
            {
                throw new NotSupportedException();
            }
            return result.Childs.Select(c => new TerrainPath(c.Contour.Select(p => new TerrainPoint(p)).ToList())).ToList();
        }

        public IEnumerable<TerrainPath> SubstractAll(IEnumerable<TerrainPolygon> others)
        {
            var result = new List<TerrainPath>() { this };
            foreach (var other in others.Where(o => GeometryHelper.EnveloppeIntersects(this, o)))
            {
                var previousResult = result.ToList();
                result.Clear();
                foreach (var subjet in previousResult)
                {
                    result.AddRange(subjet.Substract(other));
                }
            }
            return result;
        }

        public IEnumerable<TerrainPath> ClippedBy(TerrainPolygon polygon)
        {
            return Intersection(polygon);
        }
        public bool EnveloppeIntersects(ITerrainEnvelope other)
        {
            return other.MinPoint.X <= MaxPoint.X &&
                other.MinPoint.Y <= MaxPoint.Y &&
                other.MaxPoint.X >= MinPoint.X &&
                other.MaxPoint.Y >= MinPoint.Y;
        }

        internal GeoJSON.Text.Geometry.LineString ToGeoJson()
        {
            return new GeoJSON.Text.Geometry.LineString(Points);
        }

        public float Distance(TerrainPoint p)
        {
            return (float)AsLineString.Distance(new Point(p.X, p.Y));
        }

        public TerrainPoint NearestPointBoundary(TerrainPoint p)
        {
            var distance = new DistanceOp(AsLineString, new Point(p.X, p.Y));
            var segment = distance.NearestPoints();
            return new TerrainPoint((float)segment[0].X, (float)segment[0].Y);
        }


    }
}