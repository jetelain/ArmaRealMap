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
    /// Target that accepts imported compositions from the P3D model importer.
    /// Implemented by editors that allow adding object placements detected
    /// from an existing Arma 3 scene (e.g. by scanning a sample WRP/SQF).
    /// </summary>
    internal interface IModelImporterTarget
    {
        void AddComposition(Composition composition, ObjectPlacementDetectedInfos detected);
    }
}
