using GameRealisticMap.Reporting;
using WeatherStats.Databases;

namespace GameRealisticMap.Nature.Weather
{
    internal class WeatherBuilder : IDataBuilder<WeatherData>
    {
        private readonly IProgressSystem progress;

        public WeatherBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public WeatherData Build(IBuildContext context)
        {
            var db = WeatherStatsDatabase.Create("https://weatherdata.pmad.net/ERA5AVG/");

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
