using System.Numerics;
using BIS.Core.Streams;
using BIS.WRP;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Edit
{
    public class WrpEditProcessor
    {
        private readonly IProgressSystem _progressSystem;

        public WrpEditProcessor(IProgressSystem progressSystem)
        {
            _progressSystem = progressSystem;
        }

        public void ProcessAndSave(string sourceFile, string targetFile, WrpEditBatch batch)
        {
            EditableWrp source;
            using (var report = _progressSystem.CreateStep("Load", 1))
            {
                source = StreamHelper.Read<AnyWrp>(sourceFile).GetEditableWrp();
            }

            ProcessAndSave(targetFile, source, batch);
        }

        public void ProcessAndSave(string targetFile, EditableWrp world, WrpEditBatch batch)
        {
            Process(world, batch);

            using (var report = _progressSystem.CreateStep("Save", 1))
            {
                StreamHelper.Write(world, targetFile);
            }
        }

        public void Process(EditableWrp world, WrpEditBatch batch)
        {
            var objects = FilterObjects(world, batch.Remove, world.CellSize * world.LandRangeX);

            using (var report = _progressSystem.CreateStep("Objects", 1))
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

            using (var report = _progressSystem.CreateStep("Remove", objects.Count))
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
                using (var report = _progressSystem.CreateStep("RemoveRelaxed", objects.Count))
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

            return objects;
        }

        private SimpleSpacialIndex<WrpRemoveObject> BuildIndex(List<WrpRemoveObject> toRemove, float worldSize)
        {
            var toRemoveIndex = new SimpleSpacialIndex<WrpRemoveObject>(Vector2.Zero, new Vector2(worldSize));
            foreach (var hideObject in toRemove.ProgressStep(_progressSystem, "BuildIndex"))
            {
                toRemoveIndex.Insert(hideObject.Pos2D, hideObject);
            }
            return toRemoveIndex;
        }
    }
}
