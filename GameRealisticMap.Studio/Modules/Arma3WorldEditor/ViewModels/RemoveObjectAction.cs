using Gemini.Modules.UndoRedo;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class RemoveObjectAction : IUndoableAction
    {
        private Arma3WorldMapViewModel owner;
        private TerrainObjectVM obj;

        public RemoveObjectAction(Arma3WorldMapViewModel owner, TerrainObjectVM obj)
        {
            this.owner = owner;
            this.obj = obj;
        }

        public string Name => $"Remove '{obj.Model}'";

        public void Execute()
        {
            owner.EditPoints = obj;
            obj.IsRemoved = true;
        }

        public void Undo()
        {
            owner.EditPoints = obj;
            obj.IsRemoved = false;
            owner.SelectItem(obj);
        }
    }
}