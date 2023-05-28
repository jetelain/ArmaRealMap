using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Studio.Modules.Reporting
{
    internal interface IProgressTaskUI : IProgressTask
    {
        Action? DisplayResult { get; set; }
    }
}
