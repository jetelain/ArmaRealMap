using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Nature.Weather;
using WeatherStats;
using WeatherStats.Stats;

namespace GameRealisticMap.Arma3.GameEngine.Extensions
{
    public static class AceWeather
    {   
        
        // ACE wants Wind Origin : N , NE  , E   , SE  , S   , SW  , W   , NW
        private static readonly WindDirection8[] AceDirections = new[] {
            WindDirection8.South,
            WindDirection8.SouthWest,
            WindDirection8.West,
            WindDirection8.NorthWest,
            WindDirection8.North,
            WindDirection8.NorthEast,
            WindDirection8.East,
            WindDirection8.SouthEast
        };


        public static string GenerateWeather(WeatherData weatherData)
        {
            if (weatherData.Data == null)
            {
                return string.Empty;
            }

            var tempDay = weatherData.Data.Months.Select(m => Math.Round(m.Temperature.Max.Avg, 2)).ToList();
            var tempNight = weatherData.Data.Months.Select(m => Math.Round(m.Temperature.Min.Avg, 2)).ToList();
            var humidity = weatherData.Data.Months.Select(m => Math.Round(m.Humidity.Avg, 2)).ToList();
            var windSpeedMean = weatherData.Data.Months.Select(m => Math.Round(m.WindSpeed.Avg.Avg, 2)).ToList();
            var windSpeedMin = weatherData.Data.Months.Select(m => AceRandominzed(m.WindSpeed.Avg.Avg, m.WindSpeed.Min)).ToList();
            var windSpeedMax = weatherData.Data.Months.Select(m => AceRandominzed(m.WindSpeed.Avg.Avg, m.WindSpeed.Max)).ToList();
            var windDirectionProbabilities = weatherData.Data.Months.Select(m => AceDirections.Select(d => Math.Round(m.WindDirection.GetProbability(d), 2)).ToList()).ToList();

            return $@"
// Source: Copernicus Climate Change Service (C3S): ERA5: Fifth generation of ECMWF atmospheric reanalyses of the global climate. Copernicus Climate Change Service Climate Data Store (CDS)
ACE_TempDay[]   = {Serialize(tempDay)};
ACE_TempNight[] = {Serialize(tempNight)};
ACE_Humidity[]  = {Serialize(humidity)};
ACE_WindSpeedMax[]  = {Serialize(windSpeedMax)};
ACE_WindSpeedMean[] = {Serialize(windSpeedMean)};
ACE_WindSpeedMin[]  = {Serialize(windSpeedMin)};
ACE_WindDirectionProbabilities[]  = {Serialize(windDirectionProbabilities)};
";
        }

        private static string Serialize(List<double> values, string format = "0.00")
        {
            return "{" + string.Join(", ", values.Select(v => v.ToString(format, CultureInfo.InvariantCulture))) + "}";
        }

        private static string Serialize(List<List<double>> values, string format = "0.00")
        {
            return "{" + string.Join(",\r\n", values.Select(v => Serialize(v, format))) + "}";
        }

        private static List<double> AceRandominzed(float avg, MinMaxAvg value)
        {
            var span = Math.Min(Math.Abs(value.Avg - avg), Math.Max(Math.Abs(value.Max - value.Avg), Math.Abs(value.Avg - value.Min)));
            return new List<double>() { Math.Round(value.Avg, 2), Math.Round(span * 2, 2) };
        }
    }
}
