using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Studio.Modules.CompositionTool.ViewModels
{
    /// <summary>
    /// Target that accepts compositions from the asset library, or from the clipboard.
    /// </summary>
    internal interface IModelImporterTarget
    {
        void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected);
    }
}
