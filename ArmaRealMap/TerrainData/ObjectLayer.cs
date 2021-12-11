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
        public MapInfos MapInfos { get; }

        public TerrainObjectLayer(MapInfos map)
            : base(map)
        {
            MapInfos = map;
        }

        public void WriteFile(string targetFile)
        {
            var report = new ProgressReport(Path.GetFileName(targetFile), Count);
            using (var writer = new StreamWriter(new FileStream(targetFile, FileMode.Create, FileAccess.Write)))
            {
                foreach (var obj in Values)
                {
                    writer.WriteLine(obj.ToTerrainBuilderCSV());
                    report.ReportOneDone();
                }
            }
            report.TaskDone();
        }

        public void ReadFile(string sourceFile, ObjectLibraries libs)
        {
            ReadFile(sourceFile, libs.GetObject);
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
                                (float)(double.Parse(tokens[1], CultureInfo.InvariantCulture) - 200000),
                                (float)(double.Parse(tokens[2], CultureInfo.InvariantCulture)));
                    var angle = - float.Parse(tokens[3], CultureInfo.InvariantCulture);
                    var altitude = float.Parse(tokens[7], CultureInfo.InvariantCulture) - obj.CZ;

                    point = TerrainObject.TransformBack(obj, point, angle);

                    var tobj = new TerrainObject(obj,point,angle,altitude);

                    Insert(point.Vector, tobj);
                    report.ReportItemsDone((int)reader.BaseStream.Position);
                }
                report.TaskDone();
            }
        }
    }
}
