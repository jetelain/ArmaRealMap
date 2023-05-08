using ArmaRealMap.Core;
using ArmaRealMap.Core.Roads;
using GameRealisticMap.ManMade.Roads;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArmaRealMap.TerrainData.Roads
{
    public class RoadTypeLibrary
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            Converters ={
                new JsonStringEnumConverter()
            },
            WriteIndented = true
        };

        private readonly List<RoadTypeInfos> infos = new List<RoadTypeInfos>();


        public void LoadFromFile(string filename, TerrainRegion? region = null)
        {
            if (!File.Exists(filename) && Directory.Exists("P:"))
            {
                throw new ApplicationException($"'{filename}' is missing.");
            }

            infos.AddRange(JsonSerializer.Deserialize<RoadTypeInfos[]>(File.ReadAllText(filename), options));
            if (region != null) // Prefilter to speedup searchs
            {
                infos.RemoveAll(e => e.Terrain != null && e.Terrain != region.Value && e.Terrain != TerrainRegion.Unknown);
            }
        }

        public RoadTypeInfos GetInfo(RoadTypeId id, TerrainRegion region)
        {
            var entry = infos.FirstOrDefault(i => i.Id == id && i.Terrain == region) ??
                infos.FirstOrDefault(i => i.Id == id && (i.Terrain == null || i.Terrain == TerrainRegion.Unknown));
            if (entry == null)
            {
                throw new ApplicationException($"Road type is missing for '{id}' in '{region}'.");
            }
            return entry;
        }
    }
}
