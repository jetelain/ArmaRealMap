using System.Linq;
using GameRealisticMap.Arma3.GameEngine.Roads;
using GameRealisticMap.Geometries;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class SplitRoadAction : IUndoableAction
    {
        private readonly EditableArma3Road secondRoad;
        private readonly EditRoadEditablePointCollection target;
        private readonly int index;

        public SplitRoadAction(EditRoadEditablePointCollection target, int index)
        {
            var secondRoadPoints = target.Points.Skip(index).ToList();
            secondRoad = new EditableArma3Road(target.Road.Order, target.Road.RoadTypeInfos, new TerrainPath(secondRoadPoints));
            secondRoad.IsRemoved = true;
            target.Editor.Roads!.Roads.Add(secondRoad);

            this.target = target;
            this.index = index;
        }

        public string Name => "Split road";

        public void Execute()
        {
            secondRoad.IsRemoved = false;
            var newCount = index + 1;
            while (target.Points.Count > newCount)
            {
                target.Points.RemoveAt(index + 1);
            }
        }

        public void Undo()
        {
            secondRoad.IsRemoved = true;
            foreach (var point in secondRoad.Path.Points.Skip(1))
            {
                target.Points.Add(point);
            }
            target.EnsureFocus();
        }
    }
}