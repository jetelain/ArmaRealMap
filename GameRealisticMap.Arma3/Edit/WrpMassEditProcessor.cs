using System.Numerics;
using BIS.WRP;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Edit
{
    public class WrpMassEditProcessor
    {
        private readonly IProgressSystem _progressSystem;
        private readonly ModelInfoLibrary _library;

        public WrpMassEditProcessor(IProgressSystem progressSystem, ModelInfoLibrary library)
        {
            _progressSystem = progressSystem;
            _library = library;
        }

        public int Process(EditableWrp world, WrpMassEditBatch operations)
        {
            var totalChanges = 0;
            List<EditableWrpObject?> objects = world.GetNonDummyObjects().ToList();

            if (operations.Reduce.Count > 0)
            {
                foreach(var reduce in operations.Reduce.ProgressStep(_progressSystem, "Reduce"))
                {
                    totalChanges += Reduce(objects, reduce);
                }
            }

            if (operations.Replace.Count > 0)
            {
                foreach (var replace in operations.Replace.ProgressStep(_progressSystem, "Replace"))
                {
                    totalChanges += Replace(objects, replace);
                }
            }

            using (var report = _progressSystem.CreateStep("Objects", 1))
            {
                objects = objects.Where(m => m != null)
                    .Concat(new[] { EditableWrpObject.Dummy })
                    .ToList();

                var id = 1;
                foreach (var obj in objects)
                {
                    obj!.ObjectID = id;
                    id++;
                }

                world.Objects = objects;
            }
            return totalChanges;
        }

        private int Replace(List<EditableWrpObject?> objects, WrpMassReplace operation)
        {
            float xShift = (float)(operation.XShift ?? 0.0);
            float zShift = (float)(operation.ZShift ?? 0.0);

            float altShift;
            if (operation.YShift == null)
            {
                var oldModel = _library.ReadModelInfoOnly(operation.SourceModel) 
                    ?? throw new ApplicationException($"ODOL file for model '{operation.SourceModel}' was not found."); 

                var newModel = _library.ReadModelInfoOnly(operation.TargetModel) 
                    ?? throw new ApplicationException($"ODOL file for model '{operation.TargetModel}' was not found.");

                altShift = newModel.BoundingCenter.Y - oldModel.BoundingCenter.Y;
            }
            else
            {
                altShift = (float)operation.YShift.Value;
            }

            var changes = 0;
            foreach (var obj in objects)
            {
                if (obj != null && string.Equals(obj.Model, operation.SourceModel, StringComparison.OrdinalIgnoreCase))
                {
                    obj.Model = operation.TargetModel;
                    if (altShift != 0)
                    {
                        if (obj.Transform.AltitudeScale != 1f)
                        {
                            obj.Transform.Altitude += altShift * obj.Transform.AltitudeScale;
                        }
                        else
                        {
                            obj.Transform.Altitude += altShift;
                        }
                    }
                    if (xShift != 0 || zShift != 0)
                    {
                        var translate = Vector3.Transform(new Vector3(xShift, 0, zShift), obj.Transform.Matrix);
                        obj.Transform.TranslateX = translate.X;
                        obj.Transform.TranslateZ = translate.Z;
                    }
                    changes++;
                }
            }
            _progressSystem.WriteLine($"Replace '{operation.SourceModel}'->'{operation.TargetModel}' with xShift={xShift:0.00}, yShift={altShift:0.00}, zShift={zShift:0.00} -> {changes} changes");
            return changes;
        }

        private int Reduce(List<EditableWrpObject?> objects, WrpMassReduce operation)
        {
            var changes = 0;
            var rnd = RandomHelper.CreateRandom(operation.Model.ToLowerInvariant());
            for (int i = 0; i < objects.Count; ++i)
            {
                var obj = objects[i];
                if (obj != null &&
                    string.Equals(obj.Model, operation.Model, StringComparison.OrdinalIgnoreCase) &&
                    (operation.RemoveRatio == 1 || rnd.NextDouble() <= operation.RemoveRatio))
                {
                    objects[i] = null;
                    changes++;
                }
            }
            Console.WriteLine($"Reduce '{operation.Model}' -> {changes} removed");
            return changes;
        }
    }
}
