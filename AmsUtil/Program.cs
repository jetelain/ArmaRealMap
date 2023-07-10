using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using ArmaRealMap.TerrainBuilder;
using BIS.Core.Math;
using BIS.Core.Serialization;
using BIS.Core.Streams;
using BIS.P3D;
using BIS.PBO;
using BIS.WRP;
using CommandLine;

namespace TerrainBuilderUtil
{
    class Program
    {
        abstract class BaseOptions
        {
            [Option('m', "mods", Required = false, HelpText = "Base path of mods directory (by default !Workshop of Arma installation directory).")]
            public string ModsBasePath { get; set; }

            [Option('a', "auto-extract", Required = false, HelpText = "Extract missing PBO on P: drive.")]
            public bool AutoExtract { get; set; }
        }

        [Verb("convert", HelpText = "Convert an AMS export file to a Terrain Builder file.")]
        class ConvertOptions : BaseOptions
        {
            [Value(0, MetaName = "source", HelpText = "AMS export file.", Required = true)]
            public string Source { get; set; }

            [Value(1, MetaName = "target", HelpText = "Target Terrain Builder file.", Required = false)]
            public string Target { get; set; }

            [Option('t', "template", Required = false, HelpText = ".")]
            public string TemplateFile { get; set; }

            [Option('r', "report", Required = false, HelpText = ".")]
            public string ReportFile { get; set; }

            [Option('e', "libraries", Required = false, HelpText = "Existing Terrain Builder libraries.")]
            public string Libraries { get; set; }
        }

        [Verb("edit", HelpText = "Edit a WRP file using an AMS export file.")]
        class EditWrpOptions : BaseOptions
        {
            [Value(0, MetaName = "SourceWrp", HelpText = "Source WRP file.", Required = true)]
            public string Source { get; set; }

            [Value(1, MetaName = "amsExport", HelpText = "AMS export file.", Required = true)]
            public string Edit { get; set; }

            [Value(2, MetaName = "targetWrp", HelpText = "Target WRP file.", Required = true)]
            public string Target { get; set; }
        }

        [Verb("export", HelpText = "Simple EXPORT test.")]
        class ExportOptions
        {
            [Value(0, MetaName = "SourceWrp", HelpText = "Source WRP file.", Required = true)]
            public string Source { get; set; }

            [Value(1, MetaName = "targetTxt", HelpText = "Target TXT file.", Required = true)]
            public string Target { get; set; }
        }

        static int Main(string[] args)
        {
            return CommandLine.Parser.Default.ParseArguments<ConvertOptions, EditWrpOptions, ExportOptions>(args)
              .MapResult(
                (ConvertOptions opts) => Convert(opts),
                (EditWrpOptions opts) => Edit(opts),
                (ExportOptions opts) => Export(opts),
                errs => 1);
        }

        private static int Export(ExportOptions opts)
        {
            var source = StreamHelper.Read<AnyWrp>(opts.Source).GetEditableWrp();

            using (var writer = new StreamWriter(opts.Target, false))
            {
                foreach (var add in source.GetNonDummyObjects().Take(10000))
                {
                    var yaw = (float)System.Math.Atan2(add.Transform.Matrix.M13, add.Transform.Matrix.M33) * 180 / Math.PI;
                    var pitch = (float)System.Math.Asin(-add.Transform.Matrix.M23) * 180 / Math.PI;
                    var roll = (float)System.Math.Atan2(add.Transform.Matrix.M21, add.Transform.Matrix.M22) * 180 / Math.PI;
                    writer.WriteLine(FormattableString.Invariant(@$"""{Path.GetFileNameWithoutExtension(add.Model)}"";{add.Transform.Matrix.M41};{add.Transform.Matrix.M43};{-yaw};{pitch};{roll};1;{add.Transform.Matrix.M42};"));
                }
            }
            return 0;
        }

        private static int Edit(EditWrpOptions opts)
        {
            Init(opts);

            Console.WriteLine($"Read world file from '{opts.Source}'");
            var source = StreamHelper.Read<AnyWrp>(opts.Source).GetEditableWrp();

            var nextObjectId = source.GetNonDummyObjects().Max(o => o.ObjectID) + 1;
            var exportData = ReadExportData(nextObjectId, opts.Edit);

            Console.WriteLine($"Process hide");
            var objects = FilterObjects(source, exportData.ToRemove);

            Console.WriteLine($"Build objects list");
            source.Objects = objects.Where(m => m != null).Concat(exportData.Add).Concat(new[] { EditableWrpObject.Dummy }).ToList();

            Console.WriteLine($"Write to '{opts.Target}'");
            StreamHelper.Write(source, opts.Target);

            EnsureModels(opts, source.GetNonDummyObjects().Select(o => o.Model));

            return 0;
        }

        private static ExportData ReadExportData(int nextObjectId, string file, bool worldPos = true)
        {
            Console.WriteLine($"Read edit file from '{file}'");

            var entries = File.ReadAllLines(file);
            var exportData = new ExportData();
            var models = new Dictionary<string, string>();
            foreach (var entry in entries)
            {
                var array = ArmaTextDeserializer.ParseSimpleArray(entry);
                switch (array[0] as string)
                {
                    case ".hide":
                    case ".hideObj":
                        exportData.ToRemove.Add(new HideObject(GetVector((object[])array[1]), NormalizeModelPath((string)array[2])));
                        break;
                    case ".hideArea":
                        break;
                    case ".class":
                        models.Add((string)array[1], NormalizeModelPath((string)array[2]));
                        break;
                    default:
                        var model = models[(string)array[0]];
                        exportData.Add.Add(new EditableWrpObject()
                        {
                            Model = model,
                            ObjectID = nextObjectId++,
                            Transform = GetTransform(worldPos, array, model)
                        });
                        break;
                }
            }

            return exportData;
        }

        private static readonly Vector3 DefaultVectorDir = new Vector3(0, 0, 1); // North
        private static readonly Vector3 DefaultVectorUp = new Vector3(0, 1, 0); // Up
        private static readonly Vector3 DefaultVectorCross = Vector3.Cross(DefaultVectorDir, DefaultVectorUp);
        private static readonly Dictionary<string, bool> slopelandcontact = new Dictionary<string, bool>();

        private static Matrix4P GetTransform(bool useWorldPos, object[] array, string model)
        {
            // [_class, _pos, getPosWorld _x, vectorUp _x, vectorDir _x, surfaceNormal _pos];
            var position = GetVector(useWorldPos ? (object[])array[2] : (object[])array[1]);
            var vectorUp = GetVector((object[])array[3]);
            var vectorDir = GetVector((object[])array[4]);

            var matrix = Matrix4x4.CreateWorld(position,-vectorDir,vectorUp);

            if (IsSlopeLandContact(model))
            {
                // If object is SlopeLandContact, it means that engine will make matrix relative
                // to surfaceNormal so we have to compensate it
                var surfaceNormal = GetVector((object[])array[5]);
                if (surfaceNormal != DefaultVectorUp)
                {
                    var normalCompensation = Vector3.Lerp(DefaultVectorUp, surfaceNormal, -1);
                    var normalFixMatrix = Matrix4x4.CreateWorld(Vector3.Zero, -Vector3.Cross(normalCompensation, DefaultVectorCross), normalCompensation);
                    var newVectorUp = Vector3.Transform(vectorUp, normalFixMatrix);
                    var newVectorDir = Vector3.Cross(newVectorUp, Vector3.Cross(vectorDir, vectorUp)); // ensure perfectly normal to newVectorUp
                    matrix = Matrix4x4.CreateWorld(position, -newVectorDir, newVectorUp);
                }
            }
            return new Matrix4P(matrix);
        }

        private static bool IsSlopeLandContact(string model)
        {
            if (!slopelandcontact.TryGetValue(model, out var isSlopeLandContact))
            {
                var infos = StreamHelper.Read<P3D>(Path.Combine("P:", model));
                if (infos.ODOL != null)
                {
                    isSlopeLandContact = infos.ODOL.ModelInfo.LandContact > 0;
                }
                else if (infos.MLOD != null)
                {
                    // TODO
                }
                slopelandcontact.Add(model, isSlopeLandContact);
                Console.WriteLine($"{model}, IsSlopeLandContact={isSlopeLandContact}");
            }
            return isSlopeLandContact;
        }

        private static List<EditableWrpObject> FilterObjects(EditableWrp source, List<HideObject> toRemove)
        {
            var objects = source.GetNonDummyObjects().ToList();

            if (toRemove.Count == 0)
            {
                return objects;
            }

            toRemove = toRemove.Where(m => !string.IsNullOrEmpty(m.Model) && !string.IsNullOrEmpty(m.ModelName)).Distinct().ToList();

            var toRemoveIndex = new Dictionary<int, List<HideObject>>();

            foreach (var hideObject in toRemove)
            {
                var key = (int)MathF.Ceiling(hideObject.WorldPos.X);
                Add(toRemoveIndex, hideObject, key - 1);
                Add(toRemoveIndex, hideObject, key);
                Add(toRemoveIndex, hideObject, key + 1);
            }
            var removed = 0;
            for (int i = 0; i < objects.Count; ++i)
            {
                var obj = objects[i];
                var key = (int)MathF.Ceiling(obj.Transform.Matrix.M41);
                List<HideObject> list;
                if (toRemoveIndex.TryGetValue(key, out list))
                {
                    var matches = list.Where(h => h.Match(obj)).ToList();
                    if (matches.Count > 0)
                    {
                        objects[i] = null;
                        removed++;
                        foreach (var match in matches)
                        {
                            match.Matches.Add(obj);
                        }
                    }
                }
            }

            var notmatch = toRemove.Where(h => !h.HasOneMatch).ToList();
            if (notmatch.Count > 0)
            {
                for (int i = 0; i < objects.Count; ++i)
                {
                    var obj = objects[i];
                    if (obj != null)
                    {
                        var matches = notmatch.Where(h => h.MatchRelaxed(obj)).ToList();
                        if (matches.Count > 0)
                        {
                            objects[i] = null;
                            removed++;
                            foreach (var match in matches)
                            {
                                match.Matches.Add(obj);
                            }
                        }
                    }
                }
            }

            return objects;
        }

        private static void Add(Dictionary<int, List<HideObject>> toHide, HideObject hideObject, int key)
        {
            List<HideObject> list;
            if (!toHide.TryGetValue(key, out list))
            {
                list = new List<HideObject>();
                toHide.Add(key, list);
            }
            list.Add(hideObject);
        }

        private static string NormalizeModelPath(string model)
        {
            var path = model.TrimStart('\\');
            if (!path.EndsWith(".p3d", StringComparison.OrdinalIgnoreCase))
            {
                return path + ".p3d";
            }
            return path;
        }

        private static Vector3 GetVector(object[] armaVector)
        {
            return new Vector3(
                System.Convert.ToSingle(armaVector[0]),
                System.Convert.ToSingle(armaVector[2]),
                System.Convert.ToSingle(armaVector[1])
                );
        }

        private static int Convert(ConvertOptions opts)
        {
            Init(opts);

            if (string.IsNullOrEmpty(opts.Target))
            {
                opts.Target = Path.ChangeExtension(opts.Source, ".tb.txt");
            }
            if (string.IsNullOrEmpty(opts.ReportFile))
            {
                opts.ReportFile = Path.ChangeExtension(opts.Source, ".report.txt");
            }
            if (string.IsNullOrEmpty(opts.TemplateFile))
            {
                opts.TemplateFile = Path.ChangeExtension(opts.Source, ".tml");
            }

            var exportData = ReadExportData(0, opts.Source, false);

            var libs = new TBLibraries();
            if (!string.IsNullOrEmpty(opts.Libraries))
            {
                libs.LoadAllFrom(opts.Libraries);
            }

            var models = exportData.Add.Select(a => a.Model).Distinct();

            GenerateMissingTemplates(opts, libs, models);

            using (var writer = new StreamWriter(opts.Target, false))
            {
                foreach (var add in exportData.Add)
                {
                    var yaw = (float)System.Math.Atan2(add.Transform.Matrix.M13, add.Transform.Matrix.M33) * 180 / Math.PI;
                    var pitch = (float)System.Math.Asin(-add.Transform.Matrix.M23) * 180 / Math.PI;
                    var roll = (float)System.Math.Atan2(add.Transform.Matrix.M21, add.Transform.Matrix.M22) * 180 / Math.PI;
                    writer.WriteLine(FormattableString.Invariant(@$"""{libs.FindByModel(add.Model).Name}"";{200000+add.Transform.Matrix.M41};{add.Transform.Matrix.M43};{-yaw};{pitch};{roll};1;{add.Transform.Matrix.M42};"));
                }
            }

            EnsureModels(opts, models);

            return 0;
        }

        private static void Init(BaseOptions opts)
        {
            if (string.IsNullOrEmpty(opts.ModsBasePath))
            {
                opts.ModsBasePath = @"C:\Program Files (x86)\Steam\steamapps\common\Arma 3\!Workshop";
            }
        }

        private static void EnsureModels(BaseOptions opts, IEnumerable<string> allModels)
        {
            if (!opts.AutoExtract)
            {
                return;
            }
            Console.WriteLine($"Detect missing models in project drive");

            var models = allModels.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var modsData = ListMods(opts);

            var allPbos = modsData.SelectMany(m => m.Pbos);

            var usedPbo = new HashSet<PboInfo>();
            foreach (var model in models)
            {
                if (!model.StartsWith("a3\\", StringComparison.OrdinalIgnoreCase))
                {
                    var pbo = allPbos.FirstOrDefault(p => p.Files.Contains(model));
                    if (pbo != null)
                    {
                        usedPbo.Add(pbo);
                    }
                    else
                    {
                        Console.Error.WriteLine($"Model '{model}' was not found.");
                    }
                }
            }

            foreach (var pbo in usedPbo)
            {
                var missing = pbo.Files.Where(f => !File.Exists(Path.Combine("P:", f))).FirstOrDefault();
                if (missing != null)
                {
                    Console.WriteLine($"Extract '{Path.GetFileNameWithoutExtension(pbo.Path)}' because '{missing}' is missing.");
                    new PBO(pbo.Path).ExtractAllFiles("P:");
                }
            }

        }

        private static List<ModInfo> ListMods(BaseOptions opts)
        {
            var modsData = new List<ModInfo>();
            foreach (var mod in Directory.GetDirectories(opts.ModsBasePath))
            {
                var path = Path.Combine(mod, "addons");
                if (Directory.Exists(path))
                {
                    var infos = new ModInfo();
                    infos.Path = mod;
                    infos.Pbos = new List<PboInfo>();
                    var allPBOs = Directory.GetFiles(Path.Combine(mod, "addons"), "*.pbo");
                    foreach (var pboPath in allPBOs)
                    {
                        var pbo = new PBO(pboPath);
                        var pboInfos = new PboInfo();
                        pboInfos.Mod = infos;
                        pboInfos.Path = pboPath;
                        foreach (var entry in pbo.FileEntries)
                        {
                            if (string.Equals(Path.GetExtension(entry.FileName), ".p3d", StringComparison.OrdinalIgnoreCase))
                            {
                                pboInfos.Files.Add(Path.Combine(pbo.Prefix, entry.FileName));
                            }
                        }
                        if (pboInfos.Files.Count > 0)
                        {
                            infos.Pbos.Add(pboInfos);
                        }
                    }
                    if (infos.Pbos.Count > 0)
                    {
                        infos.WorkshopId = GetWorkshopId(mod);
                        modsData.Add(infos);
                    }
                }
            }

            return modsData;
        }

        private static void GenerateMissingTemplates(ConvertOptions opts, TBLibraries libs, IEnumerable<string> models)
        {
            var lib = new TBLibrary() { Name = Path.GetFileNameWithoutExtension(opts.TemplateFile), Template = new List<TBTemplate>() };
            libs.Libraries.Add(lib);
            foreach (var model in models)
            {
                if (libs.FindByModel(model) == null)
                {
                    var name = Path.GetFileNameWithoutExtension(model);

                    int suffix = 1;
                    while (libs.FindByName(name) != null)
                    {
                        name = Path.GetFileNameWithoutExtension(model) + "_" + suffix;
                        suffix++;
                    }

                    lib.Template.Add(new TBTemplate()
                    {
                        Name = name,
                        File = model,
                        BoundingCenter = new TBVector() { X = -999.0000f, Y = -999.0000f, Z = -999.0000f },
                        BoundingMax = new TBVector() { X = -999.0000f, Y = -999.0000f, Z = -999.0000f },
                        BoundingMin = new TBVector() { X = 999.0000f, Y = 999.0000f, Z = 999.0000f },
                        Height = 0,
                        Hash = TBTemplate.GenerateHash(name)
                    });
                }
            }
            Console.WriteLine($"Write template to '{opts.TemplateFile}'");
            using (var output = new StreamWriter(opts.TemplateFile, false))
            {
                TBLibraries.Serializer.Serialize(output, lib);
            }
        }


        private static readonly Regex IdRegex = new Regex(@"publishedid\s*=\s*([0-9]+);", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static string GetWorkshopId(string mod)
        {
            var infos = Path.Combine(mod, "meta.cpp");
            if (File.Exists(infos))
            {
                var match = IdRegex.Match(File.ReadAllText(infos));
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }
            return "";
        }
    }
}
