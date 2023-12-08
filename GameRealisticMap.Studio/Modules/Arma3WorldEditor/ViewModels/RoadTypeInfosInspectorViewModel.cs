using System.Collections.Generic;
using GameRealisticMap.Arma3.GameEngine.Roads;
using Gemini.Modules.Inspector.Inspectors;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class RoadTypeInfosInspectorViewModel : EditorBase<EditableArma3RoadTypeInfos>, ILabelledInspector
    {
        private readonly Arma3WorldMapViewModel parent;

        public RoadTypeInfosInspectorViewModel(Arma3WorldMapViewModel parent)
        {
            this.parent = parent;
        }

        public List<RoadTypeSelectVM> Items => parent.RoadTypes;

        public new string Name => "Type";
    }
}
