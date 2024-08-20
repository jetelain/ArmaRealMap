using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Arma3.Assets;
using GameRealisticMap.Studio.Modules.Arma3Data.Services;
using GameRealisticMap.Studio.Modules.Reporting;
using Gemini.Framework;

namespace GameRealisticMap.Studio.Modules.Arma3Data.ViewModels
{
    internal class Arma3AerialImageViewModel : WindowBase
    {
        private readonly IArma3AerialImageService service;
        private readonly IProgressTool progressTool;
        private readonly List<string> models;
        private readonly List<ModDependencyDefinition> modDependencies;

        public Arma3AerialImageViewModel(List<string> models, List<ModDependencyDefinition> modDependencies) 
            : this(IoC.Get<IProgressTool>(), IoC.Get<IArma3AerialImageService>(), models, modDependencies)
        {

        }

        public Arma3AerialImageViewModel(IProgressTool progressTool, IArma3AerialImageService service, List<string> models, List<ModDependencyDefinition> modDependencies)
        {
            this.service = service;
            this.progressTool = progressTool;
            this.models = models;
            this.modDependencies = modDependencies;
            this.CountMissing = service.CountMissing(models);
        }

        public int CountMissing { get; }

        public int MinutesForMissing => (int)Math.Floor(CountMissing * 3.25 / 60) + 1;

        public string MissingMessage => string.Format(GameRealisticMap.Studio.Labels.NImagesWillBeTakenItWillTakeUpToNMinutes, CountMissing, MinutesForMissing);

        public Task TakeAerialImages()
        {
            progressTool
                .RunTask(Labels.TakeAerialImages, progress =>
                    service.TakeImages(
                        models,
                        modDependencies,
                        progress.Scope));

            return TryCloseAsync();
        }

        public Task Cancel()
        {
            return TryCloseAsync();
        }
    }
}
