using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Caliburn.Micro;
using GameRealisticMap.Configuration;
using GameRealisticMap.Studio.Modules.Main.Services;
using Gemini.Modules.Settings;

namespace GameRealisticMap.Studio.Modules.Main.ViewModels
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export(typeof(ISettingsEditorAsync))]
    internal class GrmSourcesViewModel : PropertyChangedBase, ISettingsEditorAsync, ISourceLocations
    {
        private readonly IGrmConfigService configService;

        [ImportingConstructor]
        public GrmSourcesViewModel(IGrmConfigService configService)
        {
            this.configService = configService;
            var sources = configService.GetSources();
            MapToolkitSRTM15Plus = sources.MapToolkitSRTM15Plus;
            MapToolkitSRTM1 = sources.MapToolkitSRTM1;
            MapToolkitAW3D30 = sources.MapToolkitAW3D30;
            WeatherStats = sources.WeatherStats;
            OverpassApiInterpreter = sources.OverpassApiInterpreter;
            S2CloudlessBasePath = sources.S2CloudlessBasePath;
        }

        public string SettingsPageName => "Sources";

        public string SettingsPagePath => "Game Realistic Map";

        public Uri MapToolkitSRTM15Plus { get; set; }

        public Uri MapToolkitSRTM1 { get; set; }

        public Uri MapToolkitAW3D30 { get; set; }

        public Uri WeatherStats { get; set; }

        public Uri OverpassApiInterpreter { get; set; }

        public Uri S2CloudlessBasePath { get; set; }

        public async Task ApplyChangesAsync()
        {
            await configService.SetSources(this);
        }
    }
}
