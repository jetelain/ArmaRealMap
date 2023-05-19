using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal interface IModelImporterTarget
    {
        void AddSingleObject(ModelInfo model, ObjectPlacementDetectedInfos detected);
    }
}
