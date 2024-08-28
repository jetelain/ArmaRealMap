using GameRealisticMap.Configuration;
using Pmad.ProgressTracking;
using WeatherStats.Databases;

namespace GameRealisticMap.Nature.Weather
{
    internal class WeatherBuilder : IDataBuilderAsync<WeatherData>
    {
        private readonly ISourceLocations sources;

        public WeatherBuilder(ISourceLocations sources)
        {
            this.sources = sources;
        }

        public async Task<WeatherData> BuildAsync(IBuildContext context, IProgressScope scope)
        {
            var db = WeatherStatsDatabase.Create(sources.WeatherStats.AbsoluteUri);

            var center = context.Area.TerrainPointToLatLng(
                new Geometries.TerrainPoint(
                    context.Area.SizeInMeters / 2,
                    context.Area.SizeInMeters / 2));

            using var report = scope.CreateSingle("WeatherStats");

            var data = await db.GetStats(center.Y, center.X);

            return new WeatherData(data);
        }
    }
}
