using System.Threading.Tasks;
using ArmaRealMapWebSite.Models;
using GameRealisticMap.Arma3.GameEngine.Extensions;
using GameRealisticMap.Nature.Weather;
using Microsoft.AspNetCore.Mvc;
using WeatherStats.Databases;

namespace ArmaRealMapWebSite.Controllers
{
    public class AceWeatherController : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index(string coordinates = null)
        {
            if ( !string.IsNullOrWhiteSpace(coordinates) )
            {
                return await Index(new AceWeatherModel() { Coordinates = coordinates });
            }
            return View(new AceWeatherModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(AceWeatherModel model)
        {
            if (!CoordinateSharp.Coordinate.TryParse(model.Coordinates, out var coordinate))
            {
                ModelState.AddModelError(nameof(model.Coordinates), "Invalid coordinates");
                return View(model);
            }
            var db = WeatherStatsDatabase.Create("https://weatherdata.pmad.net/ERA5AVG/");
            var data = await db.GetStats(coordinate.Latitude.ToDouble(), coordinate.Longitude.ToDouble());
            if (data == null)
            {
                ModelState.AddModelError(nameof(model.Coordinates), "No data for those coordinates");
                return View(model);
            }
            model.RawData = data;
            model.AceConfig = AceWeather.GenerateWeather(new WeatherData(data));
            return View(model);
        }
    }
}
