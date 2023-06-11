using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GameRealisticMap.ManMade.Places
{
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
