using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Arma3.Edit
{
    public class WrpMassReplace
    {
        public WrpMassReplace(string sourceModel, string targetModel)
        {
            SourceModel = sourceModel;
            TargetModel = targetModel;
        }

        public string SourceModel { get; set; }

        public string TargetModel { get; set; }

        public double? XShift { get; set; }

        public double? ZShift { get; set; }

        public double? YShift { get; set; }
    }
}
