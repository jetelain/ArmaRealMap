using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArmaRealMap.Core.ObjectLibraries;
using BIS.Core.Streams;
using BIS.P3D;
using BIS.P3D.ODOL;

namespace ArmaRealMap
{
    public class ModelInfoLibrary
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            Converters ={
                new JsonStringEnumConverter()
            },
            WriteIndented = true
        };

        public List<ModelInfo> Models { get; } = new List<ModelInfo>();

        internal ModelInfo ResolveByName(string name)
        {
            var model = Models.FirstOrDefault(m => string.Equals(m.Name,name,StringComparison.OrdinalIgnoreCase));
            if (model == null)
            {
                throw new ApplicationException($"Unknown model '{name}'");
            }
            return model;
        }

        internal ModelInfo ResolveByPath(string path)
        {
            var model = Models.FirstOrDefault(m => string.Equals(m.Path, path, StringComparison.OrdinalIgnoreCase));
            if ( model == null)
            {
                var odol = ReadODOL(path);
                if (odol == null)
                {
                    throw new ApplicationException($"ODOL file for model '{path}' was not found, unable to use it");
                }
                model = new ModelInfo()
                {
                    Path = path,
                    Name = Path.GetFileNameWithoutExtension(path),
                    BoundingCenter = odol.ModelInfo.BoundingCenter.Vector3
                };
                Models.Add(model);
            }
            return model;
        }

        private static ODOL ReadODOL(string path)
        {
            var normal = Path.Combine("P:", path);
            if (File.Exists(normal) && P3D.IsODOL(normal))
            {
                return StreamHelper.Read<ODOL>(normal);
            }
            var temp = Path.Combine("P:\\temp", path);
            if (File.Exists(temp) && P3D.IsODOL(temp))
            {
                return StreamHelper.Read<ODOL>(temp);
            }
            return null;
        }

        internal void Load(string modelsInfoFile)
        {
            if (!string.IsNullOrEmpty(modelsInfoFile) && File.Exists(modelsInfoFile))
            {
                var data = JsonSerializer.Deserialize<JsonModelInfo[]>(File.ReadAllText(modelsInfoFile), options);

                var models = data.Where(m => m.BoundingCenterX != null)
                    .Select(m => new ModelInfo()
                    {
                        Name = m.Name,
                        Path = m.Path,
                        BoundingCenter = new Vector3(m.BoundingCenterX ?? 0, m.BoundingCenterY ?? 0, m.BoundingCenterZ ?? 0)
                    }).ToList();

                CheckBoudingCenters(models);

                Models.AddRange(models);
            }
        }

        internal void LoadAndUpdate(string modelsInfoFile)
        {
            Console.WriteLine();
            var updated = 0;
            if (!string.IsNullOrEmpty(modelsInfoFile) && File.Exists(modelsInfoFile))
            {
                var data = JsonSerializer.Deserialize<JsonModelInfo[]>(File.ReadAllText(modelsInfoFile), options);
                var report = new ProgressReport("LoadAndUpdate", data.Length);
                foreach (var model in data)
                {
                    var x = new Vector3(model.BoundingCenterX ?? 0, model.BoundingCenterY ?? 0, model.BoundingCenterZ ?? 0);
                    var odol = ReadODOL(model.Path);
                    if (odol != null)
                    {
                        if (!IsAlmostSame(odol.ModelInfo.BoundingCenter.Vector3, x))
                        {
                            updated++;
                        }
                        Models.Add(new ModelInfo() { Name = model.Name, Path = model.Path, BoundingCenter = odol.ModelInfo.BoundingCenter.Vector3 });
                    }
                    else if (model.BoundingCenterX != null)
                    {
                        Models.Add(new ModelInfo()
                        {
                            Name = model.Name,
                            Path = model.Path,
                            BoundingCenter = x
                        });
                    }
                    report.ReportOneDone();
                }
                report.TaskDone();
            }

            Console.WriteLine();
            if (updated > 0)
            {
                Console.WriteLine($"Warning: {updated} models has been fixed");
            }
            else
            {
                Console.WriteLine($"Info: All existing models were correct");
            }
            Console.WriteLine();
        }

        internal void Save(string modelsInfoFile)
        {
            File.WriteAllText(modelsInfoFile, JsonSerializer.Serialize(Models.Select(m => new JsonModelInfo()
            {
                Name = m.Name,
                Path = m.Path,
                BoundingCenterX = m.BoundingCenter.X,
                BoundingCenterY = m.BoundingCenter.Y,
                BoundingCenterZ = m.BoundingCenter.Z
            }).ToList(), options));
        }

        private static void CheckBoudingCenters(List<ModelInfo> models)
        {
            foreach (var model in models)
            {
                var odol = ReadODOL(model.Path);
                if (odol != null && !IsAlmostSame(odol.ModelInfo.BoundingCenter.Vector3, model.BoundingCenter))
                {
                    throw new ApplicationException($"BoundingCenter of '{model.Path}' mismatch: Database={model.BoundingCenter}, ODOL={odol.ModelInfo.BoundingCenter.Vector3}.");
                }
            }
        }

        private static bool IsAlmostSame(Vector3 a, Vector3 b)
        {
            return MathF.Abs(a.X - b.X) < 0.0001f
                && MathF.Abs(a.Y - b.Y) < 0.0001f 
                && MathF.Abs(a.Z - b.Z) < 0.0001f;
        }
    }
}
