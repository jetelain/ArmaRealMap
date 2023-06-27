using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.CompositionTool.ViewModels
{
    internal interface IWithCompositionRadius : IWithComposition
    {
        float Radius { get; set; }

        float FitRadius { get; set; }
    }
}
