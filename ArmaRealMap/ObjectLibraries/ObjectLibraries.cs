using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArmaRealMap.Core.ObjectLibraries;

namespace ArmaRealMap.Libraries
{
    public class ObjectLibraries
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            Converters ={
                new JsonStringEnumConverter()
            },
            WriteIndented = true
        };

        public List<ObjectLibrary> Libraries { get; } = new List<ObjectLibrary>();

        public ObjectLibraries()
        {
            
        }

        public void Load(MapConfig config, GlobalConfig global)
        {
            var libs = JsonSerializer.Deserialize<JsonObjectLibrary[]>(File.ReadAllText(global.LibrariesFile), options);

            foreach(var lib in libs)
            {
                if (lib.Terrain == null || lib.Terrain == TerrainRegion.Unknown || lib.Terrain == config.Terrain)
                {
                    Libraries.Add(new ObjectLibrary()
                    {
                        Category = lib.Category,
                        Density = lib.Density,
                        Terrain = lib.Terrain,
                        Objects = lib.Objects.Select(o => new SingleObjetInfos()
                        {
                            CX = o.CX,
                            CY = o.CY,
                            CZ = o.CZ,
                            Depth = o.Depth,
                            Height = o.Height,
                            MaxZ = o.MaxZ,
                            MinZ = o.MinZ,
                            Name = o.Name,
                            PlacementProbability = o.PlacementProbability,
                            PlacementRadius = o.PlacementRadius,
                            ReservedRadius = o.ReservedRadius,
                            Width = o.Width
                        }).ToList(),
                        Compositions = lib.Compositions?.Select(c => new CompositionInfos()
                        {
                            Height = c.Height,
                            Depth = c.Depth,
                            Width = c.Width,
                            Objects = c.Objects.Select(o => new CompositionObjetInfos()
                            {
                                Angle = o.Angle,
                                Width = o.Width,
                                Depth = o.Depth,
                                Height = o.Height,
                                Name = o.Name,
                                X = o.X,
                                Y = o.Y,
                                Z = o .Z
                            }).ToList()
                        })?.ToList()
                    }); 
                }
            }
        }

        internal SingleObjetInfos GetObject(string name)
        {
            return Libraries.SelectMany(l => l.Objects).First(o => o.Name == name);
        }
    }
}
