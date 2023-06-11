using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Studio.Modules.CompositionTool.ViewModels
{
    internal interface IWithCompositionRectangle : IWithComposition
    {
        float Width { get; set; }

        float Depth { get; set; }
    }
}
