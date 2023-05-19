using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Modules.CompositionTool.ViewModels;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.CompositionTool
{
    internal interface ICompositionTool : ITool
    {
        IWithComposition? Current { get; set; }
    }
}
