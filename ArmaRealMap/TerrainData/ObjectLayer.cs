using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArmaRealMap.Geometries;
using ArmaRealMap.Libraries;

namespace ArmaRealMap
{
    internal class TerrainObjectLayer : TerrainSpacialIndex<TerrainObject>
    {
        private readonly MapInfos map;

        public TerrainObjectLayer(MapInfos map)
            : base(map)
        {
            this.map = map;
        }

        public void WriteFile(string targetFile)
        {
            var report = new ProgressReport(Path.GetFileName(targetFile), Count);
            using (var writer = new StreamWriter(new FileStream(targetFile, FileMode.Create, FileAccess.Write)))
            {
                foreach (var obj in Values)
                {
                    writer.WriteLine(obj.ToString(map));
                    report.ReportOneDone();
                }
            }
            report.TaskDone();
        }

        public void ReadFile(string sourceFile, ObjectLibraries libs)
        {
            ReadFile(sourceFile, libs.GetObject);
        }
        public void ReadFile(string sourceFile, ObjectLibrary lib)
        {
            ReadFile(sourceFile, lib.GetObject);
        }

        public void ReadFile(string sourceFile, Func<string, SingleObjetInfos> resolveObject)
        {
            Clear();

            using (var reader = new StreamReader(new FileStream(sourceFile, FileMode.Open, FileAccess.Read)))
            {
                var report = new ProgressReport(Path.GetFileName(sourceFile), (int)reader.BaseStream.Length);
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var tokens = line.Split(';');
                    var name = tokens[0].Trim('"');
                    var obj = resolveObject(name);
                    var point = new TerrainPoint(
                                (float)(double.Parse(tokens[1], CultureInfo.InvariantCulture) - 200000 /*map.StartPointUTM.Easting*/),
                                (float)(double.Parse(tokens[2], CultureInfo.InvariantCulture) - 0 /*map.StartPointUTM.Northing*/)
                                );
                    var tobj = new TerrainObject(obj,
                            point,
                            float.Parse(tokens[3], CultureInfo.InvariantCulture));
                    Insert(point.Vector, tobj);
                    report.ReportItemsDone((int)reader.BaseStream.Position);
                }
                report.TaskDone();
            }
        }
    }
}
