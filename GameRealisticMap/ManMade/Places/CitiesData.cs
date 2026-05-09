using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GameRealisticMap.ManMade.Places
{
    /// <summary>
    /// Contains named settlement point features extracted from OSM
    /// (place=city, town, village, hamlet, suburb). Used for map labels and density adjustments.
    /// </summary>
    public class CitiesData
    {
        [JsonConstructor]
        public CitiesData(List<City> cities) 
        {
            Cities = cities;
        }

        public List<City> Cities { get; }
    }
}
