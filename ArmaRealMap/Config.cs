using System;
using System.Collections.Generic;
using System.Text;

namespace ArmaRealMap
{
    public class ConfigBottomLeft
    {
        public string GridZone { get; set; }
        public string D { get; set; }
        public int E { get; set; }
        public int N { get; set; }
    }

    public class ConfigORTHO
    {
        public string Path { get; set; }
        public string Suffix { get; set; }
        public string Extension { get; set; }
    }

    public class ConfigSRTM
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Cache { get; set; }
    }

    public class Config
    {
        public int GridSize { get; set; }
        public int CellSize { get; set; }
        public ConfigBottomLeft BottomLeft { get; set; }
        public ConfigORTHO ORTHO { get; set; }
        public string OSM { get; set; }
        public ConfigSRTM SRTM { get; set; }
    }
}
