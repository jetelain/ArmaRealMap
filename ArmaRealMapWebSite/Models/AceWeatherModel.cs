using System.ComponentModel.DataAnnotations;

namespace ArmaRealMapWebSite.Models
{
    public class AceWeatherModel
    {
        [Required]
        [Display(Description = "Coordinates")]
        public string Coordinates { get; set; }


        public string AceConfig { get; set; }
    }
}
