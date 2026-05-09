using GameRealisticMap.Arma3.GameEngine.Extensions;
using GameRealisticMap.Nature.Weather;
using Pmad.WeatherStats.Stats;
using Pmad.WeatherStats;

namespace GameRealisticMap.Arma3.Test.GameEngine.Extensions
{
    public class AceWeatherTest
    {
        // ACE direction order: N, NE, E, SE, S, SW, W, NW
        // mapped to WindDirection8:  South, SouthWest, West, NorthWest, North, NorthEast, East, SouthEast
        // WindDirection8 enum integer values: North=0, NorthEast=1, East=2, SouthEast=3, South=4, SouthWest=5, West=6, NorthWest=7

        private static readonly float[] UniformProbability = new float[8] { 0.125f, 0.125f, 0.125f, 0.125f, 0.125f, 0.125f, 0.125f, 0.125f };
        private static readonly float[] UniformSpeed      = new float[8] { 3f, 3f, 3f, 3f, 3f, 3f, 3f, 3f };

        private static MonthWeatherStatsData MakeMonth(
            float humidityAvg,
            float tempMinAvg, float tempAvgAvg, float tempMaxAvg,
            float windMinAvg, float windMinMin, float windMinMax,
            float windAvgAvg,
            float windMaxAvg, float windMaxMin, float windMaxMax,
            float[] windProbability)
        {
            var humidity    = new MinMaxAvg(humidityAvg, humidityAvg, humidityAvg);
            var tempMin     = new MinMaxAvg(tempMinAvg,  tempMinAvg,  tempMinAvg);
            var tempAvg     = new MinMaxAvg(tempAvgAvg,  tempAvgAvg,  tempAvgAvg);
            var tempMax     = new MinMaxAvg(tempMaxAvg,  tempMaxAvg,  tempMaxAvg);
            var temperature = new MinMaxAvgStats(tempMin, tempAvg, tempMax);
            var wMin        = new MinMaxAvg(windMinMin,  windMinAvg,  windMinMax);
            var wAvg        = new MinMaxAvg(windAvgAvg,  windAvgAvg,  windAvgAvg);
            var wMax        = new MinMaxAvg(windMaxMin,  windMaxAvg,  windMaxMax);
            var windSpeed   = new MinMaxAvgStats(wMin, wAvg, wMax);
            var windDir     = new WindDirectionStats(windProbability, UniformSpeed);
            return new MonthWeatherStatsData(humidity, temperature, windSpeed, windDir);
        }

        private static MonthWeatherStatsData MakeSimpleMonth(
            float humidityAvg,
            float tempMinAvg, float tempMaxAvg,
            float windAvgAvg)
        {
            return MakeMonth(
                humidityAvg,
                tempMinAvg, (tempMinAvg + tempMaxAvg) / 2f, tempMaxAvg,
                windAvgAvg, windAvgAvg, windAvgAvg,
                windAvgAvg,
                windAvgAvg, windAvgAvg, windAvgAvg,
                UniformProbability);
        }

        private static WeatherData MakeWeatherData(MonthWeatherStatsData[] months)
        {
            var point = new YearWeatherStatsPoint(46f, 6f, months);
            return new WeatherData(point);
        }

        private static WeatherData MakeUniformWeatherData()
        {
            var month = MakeSimpleMonth(70f, 5f, 20f, 3f);
            return MakeWeatherData(Enumerable.Repeat(month, 12).ToArray());
        }

        // ── null data ──────────────────────────────────────────────────────────

        [Fact]
        public void GenerateWeather_NullData_ReturnsEmptyString()
        {
            var result = AceWeather.GenerateWeather(new WeatherData(null));

            Assert.Equal(string.Empty, result);
        }

        // ── output structure ───────────────────────────────────────────────────

        [Fact]
        public void GenerateWeather_ContainsSourceComment()
        {
            var result = AceWeather.GenerateWeather(MakeUniformWeatherData());

            Assert.Contains("Copernicus Climate Change Service", result);
            Assert.Contains("ERA5", result);
        }

        [Fact]
        public void GenerateWeather_ContainsAllExpectedArrayNames()
        {
            var result = AceWeather.GenerateWeather(MakeUniformWeatherData());

            Assert.Contains("ACE_TempDay[]", result);
            Assert.Contains("ACE_TempNight[]", result);
            Assert.Contains("ACE_Humidity[]", result);
            Assert.Contains("ACE_WindSpeedMax[]", result);
            Assert.Contains("ACE_WindSpeedMean[]", result);
            Assert.Contains("ACE_WindSpeedMin[]", result);
            Assert.Contains("ACE_WindDirectionProbabilities[]", result);
        }

        // ── temperature ────────────────────────────────────────────────────────

        [Fact]
        public void GenerateWeather_TempDay_UsesTemperatureMaxAvg()
        {
            // tempDay = Temperature.Max.Avg rounded to 2 dp
            var month = MakeSimpleMonth(70f, 2f, 18.567f, 3f);
            var result = AceWeather.GenerateWeather(MakeWeatherData(Enumerable.Repeat(month, 12).ToArray()));

            // 18.567 rounded to 2dp = 18.57
            Assert.Contains("ACE_TempDay[]   = {18.57, 18.57, 18.57, 18.57, 18.57, 18.57, 18.57, 18.57, 18.57, 18.57, 18.57, 18.57}", result);
        }

        [Fact]
        public void GenerateWeather_TempNight_UsesTemperatureMinAvg()
        {
            // tempNight = Temperature.Min.Avg rounded to 2 dp
            var month = MakeSimpleMonth(70f, -3.456f, 18f, 3f);
            var result = AceWeather.GenerateWeather(MakeWeatherData(Enumerable.Repeat(month, 12).ToArray()));

            // -3.456 rounded to 2dp = -3.46
            Assert.Contains("ACE_TempNight[] = {-3.46, -3.46, -3.46, -3.46, -3.46, -3.46, -3.46, -3.46, -3.46, -3.46, -3.46, -3.46}", result);
        }

        [Fact]
        public void GenerateWeather_Temperature_VariesPerMonth()
        {
            // Jan=0, Jul=25
            var months = Enumerable.Repeat(MakeSimpleMonth(70f, 0f, 5f, 2f), 12).ToArray();
            months[6] = MakeSimpleMonth(50f, 15f, 25f, 4f);   // July (index 6)
            var result = AceWeather.GenerateWeather(MakeWeatherData(months));

            // January is index 0 = 5.00 in tempDay, July index 6 = 25.00
            Assert.Contains("ACE_TempDay[]   = {5.00, 5.00, 5.00, 5.00, 5.00, 5.00, 25.00, 5.00, 5.00, 5.00, 5.00, 5.00}", result);
        }

        // ── humidity ───────────────────────────────────────────────────────────

        [Fact]
        public void GenerateWeather_Humidity_UsesHumidityAvg()
        {
            var month = MakeSimpleMonth(65.789f, 5f, 20f, 3f);
            var result = AceWeather.GenerateWeather(MakeWeatherData(Enumerable.Repeat(month, 12).ToArray()));

            // 65.789 rounded to 2dp = 65.79
            Assert.Contains("ACE_Humidity[]  = {65.79, 65.79, 65.79, 65.79, 65.79, 65.79, 65.79, 65.79, 65.79, 65.79, 65.79, 65.79}", result);
        }

        // ── wind speed ─────────────────────────────────────────────────────────

        [Fact]
        public void GenerateWeather_WindSpeedMean_UsesWindSpeedAvgAvg()
        {
            // windSpeedMean = WindSpeed.Avg.Avg rounded to 2dp
            var month = MakeSimpleMonth(70f, 5f, 20f, 4.567f);
            var result = AceWeather.GenerateWeather(MakeWeatherData(Enumerable.Repeat(month, 12).ToArray()));

            // 4.567 rounded to 2dp = 4.57
            Assert.Contains("ACE_WindSpeedMean[] = {4.57, 4.57, 4.57, 4.57, 4.57, 4.57, 4.57, 4.57, 4.57, 4.57, 4.57, 4.57}", result);
        }

        [Fact]
        public void GenerateWeather_WindSpeedMinMax_AreRandomizedPairs()
        {
            // AceRandominzed(avg, value) → [Round(value.Avg,2), Round(span*2,2)]
            // span = Min(|value.Avg - avg|, Max(|value.Max - value.Avg|, |value.Avg - value.Min|))
            // With avg=5, wMin=MinMaxAvg(min=2, avg=3, max=4):
            //   |3-5|=2, Max(|4-3|=1, |3-2|=1)=1 → span=Min(2,1)=1 → [3.00, 2.00]
            // With avg=5, wMax=MinMaxAvg(min=6, avg=8, max=10):
            //   |8-5|=3, Max(|10-8|=2, |8-6|=2)=2 → span=Min(3,2)=2 → [8.00, 4.00]
            var month = MakeMonth(
                70f,
                5f, 12f, 20f,
                windMinAvg: 3f, windMinMin: 2f, windMinMax: 4f,
                windAvgAvg: 5f,
                windMaxAvg: 8f, windMaxMin: 6f, windMaxMax: 10f,
                UniformProbability);
            var result = AceWeather.GenerateWeather(MakeWeatherData(Enumerable.Repeat(month, 12).ToArray()));

            // Each month produces {avg, span*2} — check one representative entry per array
            Assert.Contains("ACE_WindSpeedMin[]  = {{3.00, 2.00},", result);
            Assert.Contains("ACE_WindSpeedMax[]  = {{8.00, 4.00},", result);
        }

        // ── wind direction probabilities ────────────────────────────────────────

        [Fact]
        public void GenerateWeather_WindDirectionProbabilities_OrderFollowsAceMapping()
        {
            // ACE order: N, NE, E, SE, S, SW, W, NW
            // Mapped to WindDirection8: South(4), SouthWest(5), West(6), NorthWest(7), North(0), NorthEast(1), East(2), SouthEast(3)
            // Set distinct probability per direction so we can verify the ACE mapping
            var prob = new float[8];  // indexed by WindDirection8 int
            prob[(int)WindDirection8.North]     = 0.10f; // ACE position 4 (S slot)
            prob[(int)WindDirection8.NorthEast] = 0.05f; // ACE position 5 (SW slot)
            prob[(int)WindDirection8.East]      = 0.15f; // ACE position 6 (W slot)
            prob[(int)WindDirection8.SouthEast] = 0.20f; // ACE position 7 (NW slot)
            prob[(int)WindDirection8.South]     = 0.25f; // ACE position 0 (N slot)
            prob[(int)WindDirection8.SouthWest] = 0.08f; // ACE position 1 (NE slot)
            prob[(int)WindDirection8.West]      = 0.12f; // ACE position 2 (E slot)
            prob[(int)WindDirection8.NorthWest] = 0.05f; // ACE position 3 (SE slot)
            var windDir = new WindDirectionStats(prob, UniformSpeed);

            var humidity    = new MinMaxAvg(70f, 70f, 70f);
            var tempStat    = new MinMaxAvg(10f, 10f, 10f);
            var temperature = new MinMaxAvgStats(tempStat, tempStat, tempStat);
            var wAvg        = new MinMaxAvg(3f, 3f, 3f);
            var windSpeed   = new MinMaxAvgStats(wAvg, wAvg, wAvg);
            var month       = new MonthWeatherStatsData(humidity, temperature, windSpeed, windDir);
            var result      = AceWeather.GenerateWeather(MakeWeatherData(Enumerable.Repeat(month, 12).ToArray()));

            // Expected ACE order for one month: {South, SouthWest, West, NorthWest, North, NorthEast, East, SouthEast}
            //                                 = {0.25,  0.08,      0.12, 0.05,      0.10,  0.05,      0.15, 0.20}
            Assert.Contains("{0.25, 0.08, 0.12, 0.05, 0.10, 0.05, 0.15, 0.20}", result);
        }

        [Fact]
        public void GenerateWeather_WindDirectionProbabilities_HasTwelveMonthRows()
        {
            var result = AceWeather.GenerateWeather(MakeUniformWeatherData());

            // Months are joined by ",\r\n" in the nested Serialize overload → 11 separators → 12 rows
            var start = result.IndexOf("ACE_WindDirectionProbabilities[]");
            Assert.True(start >= 0);
            var end = result.IndexOf(';', start);
            var block = result.Substring(start, end - start);
            var separatorCount = block.Split(new[] { "},\r\n" }, StringSplitOptions.None).Length - 1;
            Assert.Equal(11, separatorCount); // 11 separators between 12 month rows
        }

        // ── full output snapshot ───────────────────────────────────────────────

        [Fact]
        public void GenerateWeather_FullOutput_MatchesExpectedFormat()
        {
            // Single uniform month repeated 12 times with known values
            // humidity=70, tempMin=5, tempMax=20, windAvg=3, uniform wind direction (0.125 each → 0.13 rounded)
            var month  = MakeSimpleMonth(70f, 5f, 20f, 3f);
            var result = AceWeather.GenerateWeather(MakeWeatherData(Enumerable.Repeat(month, 12).ToArray()));

            Assert.Contains("ACE_TempDay[]   = {20.00, 20.00, 20.00, 20.00, 20.00, 20.00, 20.00, 20.00, 20.00, 20.00, 20.00, 20.00}", result);
            Assert.Contains("ACE_TempNight[] = {5.00, 5.00, 5.00, 5.00, 5.00, 5.00, 5.00, 5.00, 5.00, 5.00, 5.00, 5.00}", result);
            Assert.Contains("ACE_Humidity[]  = {70.00, 70.00, 70.00, 70.00, 70.00, 70.00, 70.00, 70.00, 70.00, 70.00, 70.00, 70.00}", result);
            Assert.Contains("ACE_WindSpeedMean[] = {3.00, 3.00, 3.00, 3.00, 3.00, 3.00, 3.00, 3.00, 3.00, 3.00, 3.00, 3.00}", result);
        }
    }
}
