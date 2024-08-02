using System.Numerics;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade;
using GameRealisticMap.ManMade.Roads;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.ManMade
{
    internal class BridgeGenerator : ITerrainBuilderLayerGenerator
    {
        private readonly IArma3RegionAssets assets;

        public BridgeGenerator(IArma3RegionAssets assets)
        {
            this.assets = assets;
        }

        public IEnumerable<TerrainBuilderObject> Generate(IArma3MapConfig config, IContext context, IProgressScope scope)
        {
            var grid = context.GetData<ElevationData>().Elevation;
            var roads = context.GetData<RoadsData>().Roads.Where(r => r.SpecialSegment == WaySpecialSegment.Bridge).WithProgress(scope, "Bridges");
            var result = new List<TerrainBuilderObject>();
            foreach(var bridge in  roads)
            {
                ProcessBridge(result, bridge, grid);
            }
            return result;
        }

        public void ProcessBridge(List<TerrainBuilderObject> objects, Road road, IElevationGrid grid)
        {
            var definition = assets.GetBridge(road.RoadType);
            if (definition != null)
            {
                ProgessBridge(objects, grid, definition, road.Path);
            }
        }

        public static void ProgessBridge(List<TerrainBuilderObject> objects, IElevationGrid grid, BridgeDefinition definition, TerrainPath path)
        {
            var elevationDelta = grid.ElevationAt(path.FirstPoint)- grid.ElevationAt(path.LastPoint);

            var delta = path.FirstPoint.Vector - path.LastPoint.Vector;
            var angle = ((MathF.Atan2(delta.Y, delta.X) * 180 / MathF.PI) + 90f) % 360f;
            var bridgeLength = elevationDelta == 0 ? path.Length : MathF.Sqrt((path.Length * path.Length) + (elevationDelta * elevationDelta));

            if (bridgeLength <= definition.Single.Size) // One object fits
            {
                SinglePartBridge(objects, path, grid, definition.Single, angle);
            }
            else // Need more
            {
                MultiPartBridge(objects, path, grid, definition, delta, angle, bridgeLength);
            }
        }

        private static void MultiPartBridge(List<TerrainBuilderObject> objects, TerrainPath path, IElevationGrid grid, BridgeDefinition definition, Vector2 delta, float angle, float bridgeLength)
        {
            var elevationStart = grid.ElevationAt(path.FirstPoint);
            var elevationEnd = grid.ElevationAt(path.LastPoint);
            var pitch = (MathF.Atan2(elevationStart - elevationEnd, delta.Length()) * 180 / MathF.PI);
            var stDelta = definition.Start.Size / 2 / bridgeLength;
            var endDelta = 1f - (definition.End.Size / 2 / bridgeLength);

            objects.AddRange(definition.Start.Model.ToTerrainBuilderObjects(
                new TerrainPoint(Vector2.Lerp(path.FirstPoint.Vector, path.LastPoint.Vector, stDelta)),
                elevationStart + ((elevationEnd - elevationStart) * (stDelta)),
                angle,
                pitch));

            var middleSize = (bridgeLength - definition.Start.Size - definition.End.Size);
            var middleParts = MathF.Ceiling(middleSize / definition.Middle.Size);
            var mPartDelta = middleSize / bridgeLength / middleParts;
            var mdelta = (definition.Start.Size / bridgeLength) + (mPartDelta/2);
            for (int n =0; n< middleParts; n++)
            {
     
                objects.AddRange(definition.Middle.Model.ToTerrainBuilderObjects(
                    new TerrainPoint(Vector2.Lerp(path.FirstPoint.Vector, path.LastPoint.Vector, mdelta)),
                    elevationStart + ((elevationEnd - elevationStart) * (mdelta)),
                    angle,
                    pitch));
                mdelta += mPartDelta;
            }
            
            objects.AddRange(definition.End.Model.ToTerrainBuilderObjects(
                new TerrainPoint(Vector2.Lerp(path.FirstPoint.Vector, path.LastPoint.Vector, endDelta)),
                elevationStart + ((elevationEnd - elevationStart) * (endDelta)),
                angle,
                pitch));
        }

        private static void SinglePartBridge(List<TerrainBuilderObject> objects, TerrainPath path, IElevationGrid grid, StraightSegmentDefinition single, float angle)
        {
            var center = new TerrainPoint(Vector2.Lerp(path.FirstPoint.Vector, path.LastPoint.Vector, 0.5f));
            var vector = Vector2.Normalize(path.LastPoint.Vector - path.FirstPoint.Vector) * single.Size / 2f;
            var realStart = center - vector;
            var realEnd = center + vector;
            var elevationStart = grid.ElevationAt(realStart);
            var elevationEnd = grid.ElevationAt(realEnd);
            var pitch = (MathF.Atan2(elevationStart - elevationEnd, (realStart.Vector - realEnd.Vector).Length()) * 180 / MathF.PI);
            var elevation = (elevationStart + elevationEnd) / 2f;

            objects.AddRange(single.Model.ToTerrainBuilderObjects(center, elevation, angle, pitch));
        }
    }
}
