using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Reporting
{
    public class ConsoleProgressSystem : IProgressSystem
    {
        public IProgressInteger CreateStep(string name, int total)
        {
            return new ConsoleProgressReport(name, total);
        }
    }
}
