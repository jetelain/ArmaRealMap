using System;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal abstract class AssetIdBase<TId, TDefinition> : AssetBase<TDefinition>
        where TId : struct, Enum 
        where TDefinition : class
    {

        protected AssetIdBase(TId id, AssetConfigEditorViewModel parent) 
            : base(parent, id.ToString())
        {
            FillId = id;
        }

        public TId FillId { get; }
    }
}
