using System.Collections;
using System.Numerics;
using BIS.Core.Serialization;
using BIS.Core.Streams;
using BIS.P3D;
using GameRealisticMap.Arma3.IO;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Arma3.Edit
{
    public class WrpEditBatchParser
    {
        private readonly IProgressSystem _progressSystem;
        private readonly IGameFileSystem _gameFileSystem;

        private static readonly Vector3 DefaultVectorDir = new Vector3(0, 0, 1); // North
        private static readonly Vector3 DefaultVectorUp = new Vector3(0, 1, 0); // Up
        private static readonly Vector3 DefaultVectorCross = Vector3.Cross(DefaultVectorDir, DefaultVectorUp);

        private readonly Dictionary<string, bool> slopelandcontact = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public WrpEditBatchParser(IProgressSystem progressSystem, IGameFileSystem gameFileSystem)
        {
            _progressSystem = progressSystem;
            _gameFileSystem = gameFileSystem;
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

        public WrpEditBatch ParseFromText(string text)
        {
            return Parse(text.Split('\n').Select(l => l.Trim()).ToList());
        }

        public WrpEditBatch ParseFromFile(string filePath)
        {
            return Parse(File.ReadAllLines(filePath));
        }

        public WrpEditBatch Parse(IReadOnlyCollection<string> entries)
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
                                exportData.Remove.Add(new WrpRemoveObject(GetTransform(array, modelHide), modelHide, objectId));
                            }
                            break;
                        case ".class":
                            var modelPath = NormalizeModelPath((string)array[2]);
                            if (!string.IsNullOrEmpty(modelPath))
                            {
                                models.Add((string)array[1], NormalizeModelPath((string)array[2]));
                            }
                            break;
                        case ".add":
                            if (models.TryGetValue((string)array[1], out var modelAdd))
                            {
                                exportData.Add.Add(new WrpAddObject(GetTransform(array, modelAdd), modelAdd));
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

                    }
                }
                report.ReportOneDone();
            }
            return exportData;
        }

        private static void ElevationData(WrpEditBatch exportData, object[] array)
        {
            exportData.Elevation.AddRange(
                (array[1] as IEnumerable)?
                .OfType<object[]>()?
                .Select(entry => new WrpSetElevationGrid(Convert.ToInt32(entry[0]), Convert.ToInt32(entry[1]), Convert.ToSingle(entry[2]))) ?? Enumerable.Empty<WrpSetElevationGrid>());
        }

        private Matrix4x4 GetTransform(object[] array, string model)
        {
            var position = GetVector((object[])array[3]);
            var vectorUp = GetVector((object[])array[4]);
            var vectorDir = GetVector((object[])array[5]);
            var matrix = Matrix4x4.CreateWorld(position, -vectorDir, vectorUp);
            if (IsSlopeLandContact(model))
            {
                // If object is SlopeLandContact, it means that engine will make matrix relative
                // to surfaceNormal so we have to compensate it
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
            return matrix;
        }

        private bool IsSlopeLandContact(string model)
        {
            if (!slopelandcontact.TryGetValue(model, out var isSlopeLandContact))
            {
                using (var file = _gameFileSystem.OpenFileIfExists(model))
                {
                    if (file != null)
                    {
                        var infos = StreamHelper.Read<P3D>(file);
                        var placement = infos.LODs.FirstOrDefault(l => l.Resolution == 1E+13f)?.NamedProperties?.FirstOrDefault(n => n.Item1 == "placement")?.Item2;
                        isSlopeLandContact = !string.IsNullOrEmpty(placement) && placement.StartsWith("slope", StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        _progressSystem.WriteLine($"Model '{model}' was not found, unknown SlopeLandContact");
                    }
                }
                slopelandcontact.Add(model, isSlopeLandContact);
            }
            return isSlopeLandContact;
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
