using System;
using System.Windows.Input;
using Caliburn.Micro;
using GameRealisticMap.Arma3.GameEngine.Roads;

namespace GameRealisticMap.Studio.Modules.Arma3WorldEditor.ViewModels
{
    internal class RoadTypeSelectVM : PropertyChangedBase, ICommand
    {
        private readonly EditableArma3RoadTypeInfos rti;
        private readonly Arma3WorldMapViewModel parent;

        public RoadTypeSelectVM(EditableArma3RoadTypeInfos rti, Arma3WorldMapViewModel parent)
        {
            this.rti = rti;
            this.parent = parent;
        }

        public bool IsSelected { get => parent.SelectectedRoadType == rti; set { } }

        public string Label => $"#{rti.Id}: {rti.Map}, {rti.TextureWidth}m";

        public string Texture => rti.Texture;

        public float TextureWidth => rti.TextureWidth;

        public event EventHandler? CanExecuteChanged;

        public EditableArma3RoadTypeInfos Value => rti;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            parent.SelectectedRoadType = rti;
        }
    }
}