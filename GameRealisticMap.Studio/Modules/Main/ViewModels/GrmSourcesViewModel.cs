using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using GameRealisticMap.Configuration;
using GameRealisticMap.Osm;
using GameRealisticMap.Studio.Modules.Main.Services;
using Gemini.Modules.Settings;
using Pmad.Cartography.Databases;

namespace GameRealisticMap.Studio.Modules.Main.ViewModels
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [Export(typeof(ISettingsEditorAsync))]
    internal class GrmSourcesViewModel : PropertyChangedBase, ISettingsEditorAsync, ISourceLocations
    {
        private readonly IGrmConfigService configService;

        /// <summary>
        /// Initializes the view model by loading the current source URLs from the configuration service.
        /// </summary>
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
            SatelliteImageProvider = sources.SatelliteImageProvider;
        }

        /// <inheritdoc />
        public string SettingsPageName => "Sources";

        /// <inheritdoc />
        public string SettingsPagePath => "Game Realistic Map";

        /// <summary>Gets or sets the base URL for the MapToolkit SRTM15Plus elevation data source.</summary>
        public Uri MapToolkitSRTM15Plus { get; set; }

        /// <summary>Gets or sets the base URL for the MapToolkit SRTM1 elevation data source.</summary>
        public Uri MapToolkitSRTM1 { get; set; }

        /// <summary>Gets or sets the base URL for the MapToolkit AW3D30 elevation data source.</summary>
        public Uri MapToolkitAW3D30 { get; set; }

        /// <summary>Gets or sets the base URL for the weather statistics data source.</summary>
        public Uri WeatherStats { get; set; }

        /// <summary>
        /// Gets or sets the URL of the Overpass API interpreter endpoint used to fetch OSM data.
        /// </summary>
        public Uri OverpassApiInterpreter { get; set; }

        /// <summary>Gets or sets the URL template for the satellite image tile provider.</summary>
        public Uri SatelliteImageProvider { get; set; }

        /// <summary>
        /// Persists the current source URLs to the configuration service.
        /// </summary>
        public async Task ApplyChangesAsync()
        {
            await configService.SetSources(this);
        }

        /// <summary>
        /// Clears the local HTTP cache for elevation data (SRTM / AW3D30) and notifies the user.
        /// </summary>
        public Task ClearElevationDataCache()
        {
            DemHttpStorage.ClearDefaultCache();
            MessageBox.Show(Labels.SourceClearElevationDataCacheDone, Labels.SourceClearElevationDataCache, MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Clears the local cache for Overpass API responses and notifies the user.
        /// <para>
        /// Note: the Overpass API enforces a 5-minute rate limit per request.
        /// Clearing the cache too frequently may cause temporary blocks.
        /// </para>
        /// </summary>
        public Task ClearOverpassDataCache()
        {
            OsmDataOverPassLoader.ClearCache();
            MessageBox.Show(Labels.SourceClearOverpassDataCacheDone, Labels.SourceClearOverpassDataCache, MessageBoxButton.OK, MessageBoxImage.Information);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Resets all source URLs to the values defined in <see cref="DefaultSourceLocations"/> and refreshes the UI bindings.
        /// </summary>
        public void ResetToDefaults()
        {
            var defaults = DefaultSourceLocations.Instance;
            MapToolkitSRTM15Plus = defaults.MapToolkitSRTM15Plus;
            MapToolkitSRTM1 = defaults.MapToolkitSRTM1;
            MapToolkitAW3D30 = defaults.MapToolkitAW3D30;
            WeatherStats = defaults.WeatherStats;
            OverpassApiInterpreter = defaults.OverpassApiInterpreter;
            SatelliteImageProvider = defaults.SatelliteImageProvider;
            NotifyOfPropertyChange(string.Empty);
        }

    }
}
