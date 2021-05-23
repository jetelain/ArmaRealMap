using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
