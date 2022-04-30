using System;
using System.IO;
using System.Linq;
using ArmaRealMap.ElevationModel;
using ArmaRealMap.TerrainData.GroundDetailTextures;
using BIS.Core.Streams;
using BIS.WRP;
using SixLabors.ImageSharp;

namespace ArmaRealMap
{
    internal static class WrpBuilder
    {
        public static void Build(MapConfig config, ElevationGrid elevationGrid, MapInfos area, GlobalConfig global, ModelInfoLibrary library)
        {
            var wrp = new EditableWrp();
            wrp.LandRangeX = 512;
            wrp.LandRangeY = 512;
            wrp.TerrainRangeX = config.GridSize;
            wrp.TerrainRangeY = config.GridSize;
            wrp.CellSize = config.GridSize / 512 * config.CellSize;

            SetElevation(config, elevationGrid, wrp);

            SetMaterials(config, new TerrainTiler(area, config), wrp);

            var files = Directory.GetFiles(config.Target.Objects, "*.txt");
            var id = 1;
            var done = 0;
            var report = new ProgressReport("Objects", (int)files.Sum(f => new FileInfo(f).Length));
            foreach (var file in files)
            {
                var mode = file.EndsWith(".abs.txt", StringComparison.OrdinalIgnoreCase) ? ElevationMode.Absolute : ElevationMode.Relative;
                foreach (var line in File.ReadAllLines(file))
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        var o = new PlacedTerrainObject(mode, line, library);
                        if (o.IsValid)
                        {
                            wrp.Objects.Add(o.ToWrpObject(id, elevationGrid));
                        }
                        id++;
                    }
                    done += line.Length + Environment.NewLine.Length;
                    report.ReportItemsDone(done);
                }
            }
            report.TaskDone();
            wrp.Objects.Add(EditableWrpObject.Dummy);

            StreamHelper.Write(wrp, Path.Combine(config.Target.Cooked, config.WorldName + ".wrp"));
        }

        private static void SetMaterials(MapConfig config, TerrainTiler terrainTiler, EditableWrp wrp)
        {
            wrp.MatNames = new string[terrainTiler.Segments.Length + 1];
            var w = terrainTiler.Segments.GetLength(0);
            var h = terrainTiler.Segments.GetLength(1);
            wrp.MatNames[0] = string.Empty;
            var report = new ProgressReport("MatNames", w);
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    wrp.MatNames[x + (y * h) + 1] = LayersHelper.GetLogicalPath(config, $"p_{x:000}-{y:000}.rvmat");
                }
                report.ReportOneDone();
            }
            report.TaskDone();

            report = new ProgressReport("MaterialIndex", 512);
            int cellPixelSize = (int)(wrp.CellSize / config.Resolution);
            var half = cellPixelSize / 2;
            wrp.MaterialIndex = new ushort[512 * 512];
            for (int x = 0; x < 512; x++)
            {
                for (int y = 0; y < 512; y++)
                {
                    var p = new Point(x * cellPixelSize + half, (511-y) * cellPixelSize + half);
                    var segment = terrainTiler.All.First(s => s.ContainsImagePoint(p));
                    wrp.MaterialIndex[x + (y * 512)] = (ushort)(segment.X + (segment.Y * h) + 1);
                }
                report.ReportOneDone();
            }
            report.TaskDone();
        }

        private static void SetElevation(MapConfig config, ElevationGrid elevationGrid, EditableWrp wrp)
        {
            var report = new ProgressReport("Elevation", config.GridSize);
            wrp.Elevation = new float[config.GridSize * config.GridSize];
            for (int x = 0; x < config.GridSize; x++)
            {
                for (int y = 0; y < config.GridSize; y++)
                {
                    wrp.Elevation[x + (y * config.GridSize)] = elevationGrid.elevationGrid[x, y];
                }
                report.ReportOneDone();
            }
            report.TaskDone();
        }
        internal static void WrpExport(WrpExportOptions options, GlobalConfig global)
        {
            var library = new ModelInfoLibrary();
            library.Load(global.ModelsInfoFile);
            var wrp = StreamHelper.Read<EditableWrp>(options.Source);
            Directory.CreateDirectory(options.Target);
            foreach (var group in wrp.GetNonDummyObjects().GroupBy(o => o.Model))
            {
                var model = library.ResolveByPath(group.Key);
                var entries = group.OrderBy(o => o.Transform.Matrix.M41).ThenBy(o => o.Transform.Matrix.M43).ThenBy(o => o.Transform.Matrix.M42).ToList();
                using (var f = File.CreateText(Path.Combine(options.Target, model.Name + ".csv")))
                {
                    f.WriteLine($"Model;M11;M12;M13;M21;M22;M23;M31;M32;M33;M41;M42;M43");
                    foreach (var entry in entries)
                    {
                        var m = entry.Transform.Matrix;
                        f.WriteLine($"{model.Name};{m.M11:0.0000};{m.M12:0.0000};{m.M13:0.0000};{m.M21:0.0000};{m.M22:0.0000};{m.M23:0.0000};{m.M31:0.0000};{m.M32:0.0000};{m.M33:0.0000};{m.M41:0.000};{m.M42:0.000};{m.M43:0.000}");
                    }
                }
                using (var f = File.CreateText(Path.Combine(options.Target, model.Name + ".abs.txt")))
                {
                    foreach (var entry in entries)
                    {
                        f.WriteLine(new PlacedTerrainObject(model, entry).ToTerrainBuilderCSV());
                    }
                }
            }
        }
    }
}
