using GameRealisticMap.Configuration;
using Pmad.ProgressTracking;
using WeatherStats.Databases;

namespace GameRealisticMap.Nature.Weather
{
    internal class WeatherBuilder : IDataBuilder<WeatherData>
    {
        private readonly ISourceLocations sources;

        public WeatherBuilder(ISourceLocations sources)
        {
            this.sources = sources;
        }

        public WeatherData Build(IBuildContext context, IProgressScope scope)
        {
            var db = WeatherStatsDatabase.Create(sources.WeatherStats.AbsoluteUri);

            var center = context.Area.TerrainPointToLatLng(
                new Geometries.TerrainPoint(
                    context.Area.SizeInMeters / 2,
                    context.Area.SizeInMeters / 2));

            using var report = scope.CreateSingle("WeatherStats");

            var data = db.GetStats(center.Y, center.X).Result;

            return new WeatherData(data);
        }
    }
}
