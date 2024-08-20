using System.Numerics;
using BIS.Core.Streams;
using BIS.WRP;
using GameRealisticMap.Arma3.GameEngine;
using GameRealisticMap.ElevationModel;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;
using Pmad.ProgressTracking;

namespace GameRealisticMap.Arma3.Edit
{
    public class WrpEditProcessor
    {
        private readonly IProgressScope _progressSystem;

        public WrpEditProcessor(IProgressScope progressSystem)
        {
            _progressSystem = progressSystem;
        }

        public void ProcessAndSave(string sourceFile, string targetFile, WrpEditBatch batch)
        {
            EditableWrp source;
            using (var report = _progressSystem.CreateSingle("Load"))
            {
                source = StreamHelper.Read<AnyWrp>(sourceFile).GetEditableWrp();
            }

            ProcessAndSave(targetFile, source, batch);
        }

        public void ProcessAndSave(string targetFile, EditableWrp world, WrpEditBatch batch)
        {
            Process(world, batch);

            using (var report = _progressSystem.CreateSingle("Save"))
            {
                StreamHelper.Write(world, targetFile);
            }
        }

        public void Process(EditableWrp world, WrpEditBatch batch)
        {
            if (batch.Elevation.Count > 0)
            {
                var delta = new ElevationGrid(world.TerrainRangeX, world.CellSize * world.LandRangeX / world.TerrainRangeX);
                using (var report = _progressSystem.CreateInteger("Elevation", batch.Elevation.Count))
                {
                    foreach (var entry in batch.Elevation)
                    {
                        var index = entry.X + (entry.Y * world.TerrainRangeX);
                        delta[entry.X, entry.Y] = entry.Elevation - world.Elevation[index];
                        world.Elevation[index] = entry.Elevation;
                        report.ReportOneDone();
                    }
                }
                if (batch.ElevationAdjustObjects)
                {
                    AdjustObjectsElevation(world, delta);
                }
            }
            
            var objects = FilterObjects(world, batch.Remove, world.CellSize * world.LandRangeX);

            using (var report = _progressSystem.CreateSingle("Objects"))
            {
                objects = objects.Where(m => m != null)
                    .Concat(batch.Add.Select(o => o.ToWrp()))
                    .Concat(new[] { EditableWrpObject.Dummy })
                    .ToList();

                var id = 1;
                foreach (var obj in objects)
                {
                    obj.ObjectID = id;
                    id++;
                }

                world.Objects = objects;
            }
        }

        public void UpdateElevationGrid(EditableWrp world, ElevationGrid newGrid)
        {
            var oldGrid = world.ToElevationGrid();
            UpdateElevationGridAbsolute(world, newGrid);
            AdjustObjectsElevation(world, GridDelta(newGrid, oldGrid));
        }

        private static ElevationGrid GridDelta(ElevationGrid newGrid, ElevationGrid oldGrid)
        {
            var size = newGrid.Size;
            var delta = new ElevationGrid(size, newGrid.CellSize.X);
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    delta[x, y] = newGrid[x, y] - oldGrid[x, y];
                }
            }
            return delta;
        }

        public void UpdateElevationGridAbsolute(EditableWrp world, ElevationGrid newGrid)
        {
            using (_progressSystem.CreateSingle("Elevation"))
            {
                world.FillFromElevationGrid(newGrid);
            }
        }

        private void AdjustObjectsElevation(EditableWrp world, ElevationGrid delta)
        {
            using (var report = _progressSystem.CreateInteger("Adjust", world.Objects.Count))
            {
                var changes = 0;

                foreach (var obj in world.Objects)
                {
                    if (!string.IsNullOrEmpty(obj.Model))
                    {
                        var ychange = delta.ElevationAt(new TerrainPoint(obj.Transform.Matrix.M41, obj.Transform.Matrix.M43));
                        if (MathF.Abs(ychange) >= 0.01f)
                        {
                            var matrix = obj.Transform.Matrix;
                            matrix.M42 += ychange;
                            obj.Transform.Matrix = matrix;
                            changes++;
                        }
                        report.ReportOneDone();
                    }
                }
                _progressSystem.WriteLine($"{changes} objects elevation was changed.");
            }
        }

        private List<EditableWrpObject> FilterObjects(EditableWrp source, List<WrpRemoveObject> toRemove, float worldSize)
        {
            var objects = source.GetNonDummyObjects().ToList();

            if (toRemove.Count == 0)
            {
                return objects;
            }

            var nomatch = new HashSet<WrpRemoveObject>(toRemove);
            var toRemoveIndex = BuildIndex(toRemove, worldSize);
            var removed = 0;

            using (var report = _progressSystem.CreateInteger("Remove", objects.Count))
            {
                for (int i = 0; i < objects.Count; ++i)
                {
                    var obj = objects[i];
                    var key = new Vector2(obj.Transform.Matrix.M41, obj.Transform.Matrix.M43);
                    var list = toRemoveIndex.Search(key - new Vector2(1f), key + new Vector2(1f));
                    if (list.Count > 0)
                    {
                        var matches = list.Where(h => h.Match(obj)).ToList();
                        if (matches.Count > 0)
                        {
                            objects[i] = null;
                            removed++;
                            foreach (var match in matches)
                            {
                                nomatch.Remove(match);
                            }
                        }
                    }
                    report.ReportOneDone();
                }
            }

            if (nomatch.Count > 0)
            {
                _progressSystem.WriteLine($"{nomatch.Count} did not match on first pass, use relaxed matching.");

                using (var report = _progressSystem.CreateInteger("RemoveRelaxed", objects.Count))
                {
                    for (int i = 0; i < objects.Count; ++i)
                    {
                        var obj = objects[i];
                        if (obj != null)
                        {
                            var matches = nomatch.Where(h => h.MatchRelaxed(obj)).ToList();
                            if (matches.Count > 0)
                            {
                                objects[i] = null;
                                removed++;
                                foreach (var match in matches)
                                {
                                    nomatch.Remove(match);
                                }
                            }
                        }
                        report.ReportOneDone();
                    }
                }
            }

            _progressSystem.WriteLine($"{removed} objects removed, for {toRemove.Count} requested.");

            return objects;
        }

        private SimpleSpacialIndex<WrpRemoveObject> BuildIndex(List<WrpRemoveObject> toRemove, float worldSize)
        {
            var toRemoveIndex = new SimpleSpacialIndex<WrpRemoveObject>(Vector2.Zero, new Vector2(worldSize));
            foreach (var hideObject in toRemove.WithProgress(_progressSystem, "BuildIndex"))
            {
                toRemoveIndex.Insert(hideObject.Pos2D, hideObject);
            }
            return toRemoveIndex;
        }
    }
}
