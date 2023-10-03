using System.ComponentModel.DataAnnotations;
using WeatherStats.Stats;

namespace ArmaRealMapWebSite.Models
{
    public class AceWeatherModel
    {
        [Required]
        [Display(Description = "Coordinates")]
        public string Coordinates { get; set; }


        public string AceConfig { get; set; }
        public YearWeatherStatsPoint RawData { get; internal set; }
    }
}
