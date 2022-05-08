using System.Collections.Generic;
using ArmaRealMap.Core;
using ArmaRealMap.Core.ObjectLibraries;
using ArmaRealMapWebSite.Entities.Assets;

namespace ArmaRealMapWebSite.Models
{
    public class LibrariesViewModel
    {
        public List<ObjectLibrary> Libraries { get; internal set; }

        public TerrainRegion? TerrainRegion { get; set; }
    }
}
