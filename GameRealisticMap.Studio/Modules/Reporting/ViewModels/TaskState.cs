using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.Reporting.ViewModels
{
    internal enum TaskState
    {
        None,

        Running,
        Canceling,

        Canceled,
        Failed,
        Done
    }
}
