using System.Collections.Generic;
using System.Linq;
using GameRealisticMap.Geometries;
using GameRealisticMap.Studio.Controls;
using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class MergeRoadAction : IUndoableAction
    {
        private readonly Arma3WorldMapViewModel owner;
        private readonly EditRoadEditablePointCollection road1;
        private readonly EditRoadEditablePointCollection road2;
        private readonly List<TerrainPoint> road2Snapshot;
        private readonly List<TerrainPoint> road1Snapshot;
        private readonly List<TerrainPoint> merged;

        public MergeRoadAction(Arma3WorldMapViewModel owner, EditRoadEditablePointCollection road1, EditRoadEditablePointCollection road2)
        {
            this.owner = owner;
            this.road1 = road1;
            this.road2 = road2;
            road1Snapshot = road1.Points.ToList();
            road2Snapshot = road2.Points.ToList();
            merged = TerrainPathHelper.AutoMergeNotOriented(road1Snapshot, road2Snapshot);
        }

        public string Name => "Merge roads";

        public void Execute()
        {
            road1.Points.Clear();
            road2.Points.Clear();
            owner.EditPoints = road1;
            road1.Points.AddRange(merged);
        }

        public void Undo()
        {
            road1.Points.Clear();
            owner.EditPoints = road1;
            road1.Points.AddRange(road1Snapshot);
            road2.Points.AddRange(road2Snapshot);
            owner.SelectedItems = new List<IEditablePointCollection> { road1, road2 };
        }
    }
}