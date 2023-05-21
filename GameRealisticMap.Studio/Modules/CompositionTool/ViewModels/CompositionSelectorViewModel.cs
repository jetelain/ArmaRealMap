using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using GameRealisticMap.Algorithms;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Arma3.Assets.Detection;
using GameRealisticMap.Studio.Modules.Arma3Data;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.CompositionTool.ViewModels
{
    internal class CompositionSelectorViewModel : WindowBase
    {
        private readonly IModelImporterTarget target;
        private Composition? clipboardComposition;
        private ObjectPlacementDetectedInfos? clipboardDetected;

        public CompositionSelectorViewModel(IModelImporterTarget target)
        {
            this.target = target;
        }

        public string ClipboardError { get; set; }

        public bool IsClipboardNotValid => !IsClipboardValid;

        public bool IsClipboardValid => clipboardComposition != null && clipboardDetected != null;

        public Task ClipboardRefresh()
        {
            clipboardComposition = null;
            ClipboardError = "Clipboard does not contains recognized data";

            var value = Clipboard.GetText();
            if (!string.IsNullOrEmpty(value))
            {
                var lines = value.Split('\n').Select(l => l.Trim()).Select(l => l.Trim()).Where(l => !string.IsNullOrEmpty(l)).ToList();
                if (lines.Count > 0 && lines.All(l => l.Split(';').Length >= 8))
                {
                    try
                    {
                        var library = IoC.Get<IArma3DataModule>().Library;
                        clipboardComposition = Composition.CreateFromCsv(lines, library).Translate(new Vector3(0, -5, 0)); // Translate z -= 5 m (assume VR world)
                        clipboardDetected = ObjectPlacementDetectedInfos.CreateFromComposition(clipboardComposition, library);
                        if (clipboardDetected == null)
                        {
                            ClipboardError = "Unable to detect size and position";
                        }
                        else
                        {
                            ClipboardError = string.Empty;
                        }
                    }
                    catch(Exception e)
                    {
                        ClipboardError = e.Message;
                    }
                }
            }

            NotifyOfPropertyChange(nameof(IsClipboardValid));
            NotifyOfPropertyChange(nameof(IsClipboardNotValid));
            NotifyOfPropertyChange(nameof(ClipboardError));
            return Task.CompletedTask;
        }

        public Task ClipboardImport()
        {
            if ( clipboardComposition != null && clipboardDetected != null)
            {
                target.AddComposition(clipboardComposition, clipboardDetected);
            }
            return TryCloseAsync(true);
        }

        protected override async Task OnActivateAsync(CancellationToken cancellationToken)
        {
            await ClipboardRefresh();
        }

        public Task Cancel() => TryCloseAsync(false);
    }
}