using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.TerrainBuilder;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class PondViewModel : PropertyChangedBase
    {
        private readonly IModelInfoLibrary _library;
        public PondViewModel(PondSizeId id, ModelInfo? modelInfo, IModelInfoLibrary library)
        {
            Id = id;
            _modelInfo = modelInfo;
            _library = library;
        }

        public PondSizeId Id { get; }

        public string IdText => Id.ToString();

        private ModelInfo? _modelInfo;
        public string? Model 
        { 
            get { return _modelInfo?.Path; } 
            set 
            {
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        _modelInfo = _library.ResolveByPath(value);
                    }
                    catch
                    {
                        // Ignore any error
                    }
                }
                else
                {
                    _modelInfo = null;
                }
                NotifyOfPropertyChange();
            } 
        }

        public ModelInfo ToDefinition()
        {
            return _modelInfo!;
        }
    }
}