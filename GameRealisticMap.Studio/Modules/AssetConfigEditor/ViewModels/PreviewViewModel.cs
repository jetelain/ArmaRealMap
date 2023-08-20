using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Arma3.TerrainBuilder;
using GameRealisticMap.Studio.Modules.Arma3Data;
using GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels.Filling;

namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.ViewModels
{
    internal class PreviewViewModel : PropertyChangedBase
    {
        public List<PreviewItem> Items { get; private set; } = new List<PreviewItem>();

        private bool isWorking;
        public bool IsWorking 
        { 
            get { return isWorking; } 
            private set { isWorking = value; NotifyOfPropertyChange(); } 
        }

        private string status = string.Empty;
        public string Status 
        { 
            get { return status; }
            private set { status = value; NotifyOfPropertyChange(); } 
        }

        public void SetPreview(List<TerrainBuilderObject> objects, string status = "")
        {
            var helper = IoC.Get<IArma3DataModule>().ModelPreviewHelper;
            Status = Labels.LoadingObjectsShapes;
            Items =
                objects.SelectMany(o => helper.ToVisualAxisY(o).Select(p =>new PreviewItem(p, o.Model, o.Scale, true)))
                .Concat(objects.SelectMany(o => helper.ToGeoAxisY(o).Select(p => new PreviewItem(p, o.Model, o.Scale, false))))
                .ToList();
            Status = status;
            IsWorking = false;
            NotifyOfPropertyChange(nameof(Items));
        }

        public void GeneratePreview(Func<(List<TerrainBuilderObject>, string)> call)
        {
            if (!IsWorking)
            {
                Status = Labels.GeneratingPreview;
                IsWorking = true;
                Task.Run(() => { var (obj, status) = call(); SetPreview(obj, status); });
            }
        }

        public void GeneratePreview(Func<List<TerrainBuilderObject>> call)
        {
            if (!IsWorking)
            {
                Status = Labels.GeneratingPreview;
                IsWorking = true;
                Task.Run(() => { SetPreview(call()); });
            }
        }
    }
}
