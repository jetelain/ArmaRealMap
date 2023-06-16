using System;
using System.IO;
using GameRealisticMap.Arma3.Assets;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class ImportConfigCommand : ICommandWithLabel
    {
        private readonly string builtin;
        private readonly AssetConfigEditorViewModel target;

        public ImportConfigCommand(string builtin, AssetConfigEditorViewModel target)
        {
            this.builtin = builtin;
            this.target = target;
        }

        public string Label => Path.GetFileNameWithoutExtension(builtin.Substring(Arma3Assets.BuiltinPrefix.Length));

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (target.CanCopyFrom)
            {
                target.CopyFrom(builtin);
            }
        }
    }
}