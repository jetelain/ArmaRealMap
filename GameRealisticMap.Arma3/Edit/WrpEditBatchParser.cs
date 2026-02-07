using System.Collections;
using System.Numerics;
using BIS.Core.Serialization;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Edit
{
    public class WrpEditBatchParser
    {
        private readonly IProgressSystem _progressSystem;
        private readonly IModelInfoLibrary _library;

        private static readonly Vector3 DefaultVectorDir = new Vector3(0, 0, 1); // North
        private static readonly Vector3 DefaultVectorUp = new Vector3(0, 1, 0); // Up
        private static readonly Vector3 DefaultVectorCross = Vector3.Cross(DefaultVectorDir, DefaultVectorUp);

        public WrpEditBatchParser(IProgressSystem progressSystem, IModelInfoLibrary library)
        {
            _progressSystem = progressSystem;
            _library = library;
        }

        private static string NormalizeModelPath(string model)
        {
            if (string.IsNullOrEmpty(model))
            {
                return model;
            }
            var path = model.TrimStart('\\');
            if (!path.EndsWith(".p3d", StringComparison.OrdinalIgnoreCase))
            {
                return path + ".p3d";
            }
            return path;
        }

        public WrpEditBatch ParseFromText(string text, SlopeLandContactBehavior contactBehavior = SlopeLandContactBehavior.TryToCompensate)
        {
            return Parse(text.Split('\n').Select(l => l.Trim()).ToList(), contactBehavior);
        }

        public WrpEditBatch ParseFromFile(string filePath, SlopeLandContactBehavior contactBehavior = SlopeLandContactBehavior.TryToCompensate)
        {
            return Parse(File.ReadAllLines(filePath), contactBehavior);
        }

        public WrpEditBatch Parse(IReadOnlyCollection<string> entries, SlopeLandContactBehavior contactBehavior = SlopeLandContactBehavior.TryToCompensate)
        {
            using var report = _progressSystem.CreateStep("Parse", entries.Count);
            var exportData = new WrpEditBatch();
            var models = new Dictionary<string, string>();
            foreach (var entry in entries)
            {
                var array = ArmaTextDeserializer.ParseSimpleArray(entry);
                if (array != null && array.Length > 1)
                {
                    switch (array[0] as string)
                    {
                        case ".map":
                            exportData.WorldName = (string)array[1];
                            exportData.WorldSize = Convert.ToSingle(array[2]);
                            exportData.Revision = Convert.ToInt32(array[3]);
                            break;
                        case ".hide":
                            var modelHide = NormalizeModelPath((string)array[1]);
                            var objectId = Convert.ToInt32(array[8]);
                            if (!string.IsNullOrEmpty(modelHide))
                            {
                                exportData.Remove.Add(new WrpRemoveObject(GetTransform(array, modelHide, contactBehavior), modelHide, objectId));
                            }
                            break;
                        case ".class":
                            var modelPath = NormalizeModelPath((string)array[2]);
                            if (!string.IsNullOrEmpty(modelPath))
                            {
                                models.Add((string)array[1], NoLandContact(modelPath, contactBehavior));
                            }
                            break;
                        case ".add":
                            if (models.TryGetValue((string)array[1], out var modelAdd))
                            {
                                exportData.Add.Add(new WrpAddObject(GetTransform(array, modelAdd, contactBehavior), modelAdd));
                            }
                            break;

                        case ".dhmap": 
                            exportData.ElevationAdjustObjects = true;
                            ElevationData(exportData, array);
                            break;

                        case ".hmap":
                            exportData.ElevationAdjustObjects = false;
                            ElevationData(exportData, array);
                            break;

                        case ".part":
                            exportData.PartIndexes.Add(Convert.ToInt32(array[1]));
                            exportData.PartCount = Convert.ToInt32(array[2]);
                            break;

                    }
                }
                report.ReportOneDone();
            }
            return exportData;
        }

        internal string NoLandContact(string path, SlopeLandContactBehavior contactBehavior)
        {
            if (contactBehavior == SlopeLandContactBehavior.Ignore)
            {
                return path;
            }
            return _library.TryGetNoLandContact(path) ?? path;
        }

        private static void ElevationData(WrpEditBatch exportData, object[] array)
        {
            exportData.Elevation.AddRange(
                (array[1] as IEnumerable)?
                .OfType<object[]>()?
                .Select(entry => new WrpSetElevationGrid(Convert.ToInt32(entry[0]), Convert.ToInt32(entry[1]), Convert.ToSingle(entry[2]))) ?? Enumerable.Empty<WrpSetElevationGrid>());
        }

        internal Matrix4x4 GetTransform(object[] array, string model, SlopeLandContactBehavior contactBehavior = SlopeLandContactBehavior.TryToCompensate)
        {
            var position = GetVector((object[])array[3]);
            var vectorUp = GetVector((object[])array[4]);
            var vectorDir = GetVector((object[])array[5]);
            var matrix = Matrix4x4.CreateWorld(position, -vectorDir, vectorUp);
            if (contactBehavior != SlopeLandContactBehavior.Ignore && IsSlopeLandContact(model))
            {
                // If object is SlopeLandContact, it means that engine will make matrix relative
                // to surfaceNormal so we have to compensate it
                if (contactBehavior == SlopeLandContactBehavior.TryToCompensate)
                {
                    var surfaceNormal = GetVector((object[])array[6]);
                    if (surfaceNormal != DefaultVectorUp)
                    {
                        var normalCompensation = Vector3.Lerp(DefaultVectorUp, surfaceNormal, -1);
                        var normalFixMatrix = Matrix4x4.CreateWorld(Vector3.Zero, -Vector3.Cross(normalCompensation, DefaultVectorCross), normalCompensation);
                        var newVectorUp = Vector3.Transform(vectorUp, normalFixMatrix);
                        var newVectorDir = Vector3.Cross(newVectorUp, Vector3.Cross(vectorDir, vectorUp)); // ensure perfectly normal to newVectorUp
                        matrix = Matrix4x4.CreateWorld(position, -newVectorDir, newVectorUp);
                    }
                }
                else if ( contactBehavior == SlopeLandContactBehavior.FollowTerrain)
                {
                    // Remove pitch/roll rotations as the game engine will make the object to follow the terrain
                    var tbInitial = new WrpAddObject(matrix, model).ToTerrainBuilder(_library);
                    var tbFixed = new TerrainBuilderObject(tbInitial.Model, tbInitial.Point, tbInitial.Elevation, tbInitial.ElevationMode, tbInitial.Yaw, 0, 0, tbInitial.Scale);
                    matrix = tbFixed.ToWrpTransform();
                }
            }
            if (array.Length > 7)
            {
                var scale = System.Convert.ToSingle(array[7]);
                if (MathF.Abs(scale - 1) > 0.001)
                {
                    matrix = Matrix4x4.CreateScale(scale) * matrix;
                }
            }
            return matrix;
        }

        private bool IsSlopeLandContact(string model)
        {
            var isSlopeLandContact = _library.IsSlopeLandContact(model);
            if (isSlopeLandContact == null)
            {
                _progressSystem.WriteLine($"Model '{model}' was not found, unknown SlopeLandContact");
            }
            return isSlopeLandContact ?? false;
        }

        private Vector3 GetVector(object[] armaVector)
        {
            return new Vector3(
                System.Convert.ToSingle(armaVector[0]),
                System.Convert.ToSingle(armaVector[2]),
                System.Convert.ToSingle(armaVector[1])
                );
        }
    }
}
