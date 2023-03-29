using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.Buildings
{
    public class BuildingsData : ITerrainData
    {
        public BuildingsData(List<Building> buildings)
        {
            Buildings = buildings;
        }

        public List<Building> Buildings { get; }
    }
}
