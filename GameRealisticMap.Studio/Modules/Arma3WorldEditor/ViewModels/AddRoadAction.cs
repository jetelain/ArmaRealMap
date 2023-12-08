using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Geometries;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class AddRoadAction : IUndoableAction
    {
        private readonly Arma3WorldMapViewModel target;
        private readonly EditableArma3Road road;
        private readonly EditRoadEditablePointCollection edit;

        public AddRoadAction(Arma3WorldMapViewModel target, EditableArma3Roads roads, EditableArma3RoadTypeInfos roadType, TerrainPoint point)
        {
            this.target = target;
            road = new EditableArma3Road(roadType.Id, roadType,new TerrainPath(point));
            road.IsRemoved = true;
            roads.Roads.Add(road);
            edit = target.CreateEdit(road);
        }

        public string Name => "Add road";

        public void Execute()
        {
            target.EditPoints = edit;
        }

        public void Undo()
        {
            target.EditPoints = null;
        }
    }
}
