using WeatherStats;
using WeatherStats.Stats;

namespace GameRealisticMap.Nature.Weather
{
    public class WeatherData
    {
        public WeatherData(YearWeatherStatsPoint? data)
        {
            Data = data;
        }

        public YearWeatherStatsPoint? Data { get; }

        public float GetPrevailingWindAngle()
        {
            if ( Data == null )
            {
                return ToAngle(WindDirection8.West);
            }
            var perMonth = Data.Months.Select(m => m.WindDirection.Prevailing).ToList();
            return ToAngle(perMonth.Distinct().OrderByDescending(m => perMonth.Count(e => e == m)).First());
        }

        private static float ToAngle(WindDirection8 direction)
        {
            switch (direction)
            {
                case WindDirection8.North:
                    return 0;
                case WindDirection8.NorthEast:
                    return -45;
                case WindDirection8.East:
                    return -90;
                case WindDirection8.SouthEast:
                    return -135;
                case WindDirection8.South:
                    return 180;
                case WindDirection8.NorthWest:
                    return 45;
                case WindDirection8.West:
                    return 90;
                case WindDirection8.SouthWest:
                    return 135;
            }
            return -90;
        }
    }
}
