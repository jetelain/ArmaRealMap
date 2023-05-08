using System;
using System.Diagnostics;
using System.Numerics;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Buildings;
using GameRealisticMap.ManMade.Roads;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.ManMade
{
    internal class BuildingGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IProgressSystem progress;
        private readonly IArma3RegionAssets assets;

        public BuildingGenerator(IProgressSystem progress, IArma3RegionAssets assets)
        {
            this.progress = progress;
            this.assets = assets;
        }

        private class Placed : IModelPosition
        {
            public Placed(Composition composition, BoundingBox box)
            {
                Composition = composition;
                Box = box;
            }

            public Composition Composition { get; }

            public BoundingBox Box { get; }

            public float Angle => Box.Angle;

            public TerrainPoint Center => Box.Center;

            public float RelativeElevation => 0;

            public float Scale => 1;

            public IEnumerable<TerrainBuilderObject> ToObjects()
            {
                return Composition.ToTerrainBuilderObjects(this);
            }
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context)
        {
            var result = new List<Placed>();
            var buildings = context.GetData<BuildingsData>().Buildings;
            var roads = context.GetData<RoadsData>().Roads;

            var nonefits = 0;
            using var report = progress.CreateStep("PlaceBuildings", buildings.Count);
            foreach (var building in buildings)
            {
                if (!TryPlaceBuilding(result, roads, building, 0.5f, 1.25f)
                    && !TryPlaceBuildingIfNotOverlapping(buildings, result, roads, building, 1.25f, 10f))
                {
                    Trace.WriteLine($"Nothing fits {building.TypeId} {building.Box.Width} x {building.Box.Height}");
                    nonefits++;
                }
                report.ReportOneDone();
            }
            return result.SelectMany(p => p.ToObjects());
        }

        private bool TryPlaceBuildingIfNotOverlapping(List<Building> wanted, List<Placed> buildings, List<Road> roads, Building building, float min, float max)
        {
            var candidates = GetBuildings(building, min, max);
            if (candidates.Count > 0)
            {
                var obj = PickOne(building, candidates);
                var delta = obj.RotateToFit(building.Box, min, max);
                var realbox = RealBoxAdjustedToRoad(roads, obj.Size, delta != 0.0f ? building.Box.RotateM90() : building.Box);
                if (!wanted.Where(b => b != building).Any(b => b.Box.Poly.Intersects(realbox.Poly))
                    && !buildings.Any(b => b.Box.Poly.Intersects(realbox.Poly)))
                {
                    buildings.Add(new Placed(obj.Composition, realbox));
                    return true;
                }
            }
            return false;
        }


        private bool TryPlaceBuilding(List<Placed> buildings, List<Road> roads, Building building, float min, float max)
        {
            var candidates = GetBuildings(building, min, max);
            if (candidates.Count > 0)
            {
                var obj = PickOne(building, candidates);
                var delta = obj.RotateToFit(building.Box, min, max);
                var box = delta != 0.0f ? building.Box.RotateM90() : building.Box;
                if (box.Width < obj.Size.X || box.Height < obj.Size.Y) // Object is larger than wanted box
                {
                    box = RealBoxAdjustedToRoad(roads, obj.Size, box);
                }
                buildings.Add(new Placed(obj.Composition, box));
                return true;
            }
            return false;
        }

        private BuildingDefinition PickOne(Building building, List<BuildingDefinition> candidates)
        {
            var random = new Random((int)Math.Truncate(building.Box.Center.X + building.Box.Center.Y));
            var obj = candidates[random.Next(0, candidates.Count)];
            return obj;
        }

        private List<BuildingDefinition> GetBuildings(Building building, float min, float max)
        {
            var match = assets.GetBuildings(building.TypeId).Where(b => b.Fits(building.Box, min, max))
               .ToList()
               .OrderBy(c => Math.Abs(c.Surface - building.Box.Surface))
               .ToList();
            if (match.Count > 0)
            {
                var first = match[0];
                var tolerance = first.Surface * 0.1f; // up to 10% from best match to have a bit of random
                return match.Where(m => Math.Abs(m.Surface - first.Surface) < tolerance).ToList();
            }
            return match;
        }

        private BoundingBox RealBoxAdjustedToRoad(List<Road> roads, Vector2 size, BoundingBox box)
        {
            // Check if real-box intersects road
            var realbox = new BoundingBox(box.Center, size.X, size.Y, box.Angle);
            var conflicts = roads
                .Where(r => r.EnveloppeIntersects(realbox))
                .Where(r => r.Polygons.Any(p => p.AsPolygon.Intersects(realbox.Poly)))
                .ToList();
            if (conflicts.Count > 0)
            {
                var dw = Math.Max(0, size.X - box.Width) / 2;
                var dh = Math.Max(0, size.Y - box.Height) / 2;
                var rotate = Matrix3x2.CreateRotation(MathF.PI * box.Angle / 180f, Vector2.Zero);
                var b1 = new BoundingBox(box.Center + Vector2.Transform(new Vector2(dw, dh), rotate), size.X, size.Y, box.Angle);
                var b2 = new BoundingBox(box.Center + Vector2.Transform(new Vector2(dw, -dh), rotate), size.X, size.Y, box.Angle);
                var b3 = new BoundingBox(box.Center + Vector2.Transform(new Vector2(-dw, -dh), rotate), size.X, size.Y, box.Angle);
                var b4 = new BoundingBox(box.Center + Vector2.Transform(new Vector2(-dw, dh), rotate), size.X, size.Y, box.Angle);
                realbox = (new[] { b1, b2, b3, b4 })
                    .Select(b => new { Box = b, Conflits = conflicts.Sum(c => c.Polygons.Sum(p => p.AsPolygon.Intersection(b.Poly).Area)) })
                    .ToList()
                    .OrderBy(b => b.Conflits).First().Box;
            }
            return realbox;
        }
    }
}
