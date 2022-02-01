using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using ArmaRealMap.Core.ObjectLibraries;

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

    public class TargetConfig
    {
        public string PboPrefix { get; set; }
        public string Terrain { get; set; }
        public string Debug { get; set; }
        public string Cache { get; set; }

        [JsonIgnore]
        public string Roads => Path.Combine(Config ?? string.Empty, "data", "roads");
        [JsonIgnore]
        public string GroundDetailTextures => Path.Combine(Config ?? string.Empty, "data", "gdt");
        [JsonIgnore]
        public string Config => PboPrefix != null ? Path.Combine("P:\\", PboPrefix) : string.Empty;
        public string GetTerrain(string name) => Path.Combine(Terrain ?? string.Empty, name);
        public string GetCache(string name) => Path.Combine(Cache ?? string.Empty, name);
        public string GetDebug(string name) => Path.Combine(Debug ?? string.Empty, name);
        public string GetLayer(string name) => Path.Combine(Config, "data", "layers", name).Replace("P:", @"C:\Users\Julien\source\repos\ArmaRealMap\PDrive");
    }

    public class Config
    {
        public int GridSize { get; set; }
        public int CellSize { get; set; }
        public double? Resolution { get; set; }
        public ConfigBottomLeft BottomLeft { get; set; }
        public ConfigORTHO ORTHO { get; set; }
        public string OSM { get; set; }
        public ConfigSRTM SRTM { get; set; }

        public string Libraries { get; set; }

        public TargetConfig Target { get; set; }

        public TerrainRegion Terrain { get; set; }
        public string SharedCache { get; set; }

        public int TileSize { get; set; } = 1024;

        public int TileOverlap { get; set; } = 16;

        public bool GenerateSatTiles { get; set; } = true;

        public bool ConvertPAA { get; set; } = true;

        public double? Scale { get; set; }

        public bool IsScaled => Scale != null;

        public ReservedArea[] Reserved { get; set; }
    }
}
