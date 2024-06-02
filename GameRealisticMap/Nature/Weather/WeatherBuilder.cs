using GameRealisticMap.Configuration;
using GameRealisticMap.Reporting;
using WeatherStats.Databases;

namespace GameRealisticMap.Nature.Weather
{
    internal class WeatherBuilder : IDataBuilder<WeatherData>
    {
        private readonly IProgressSystem progress;
        private readonly ISourceLocations sources;

        public WeatherBuilder(IProgressSystem progress, ISourceLocations sources)
        {
            this.progress = progress;
            this.sources = sources;
        }

        public WeatherData Build(IBuildContext context)
        {
            var db = WeatherStatsDatabase.Create(sources.WeatherStats.AbsoluteUri);

            var center = context.Area.TerrainPointToLatLng(
                new Geometries.TerrainPoint(
                    context.Area.SizeInMeters / 2,
                    context.Area.SizeInMeters / 2));

            using var report = progress.CreateStep("WeatherStats", 1);

            var data = db.GetStats(center.Y, center.X).Result;

            return new WeatherData(data);
        }
    }
}
