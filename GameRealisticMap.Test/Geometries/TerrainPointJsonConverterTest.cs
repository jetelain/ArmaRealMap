using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;

namespace GameRealisticMap.Test.Geometries
{
    public class TerrainPointJsonConverterTest
    {

        [Fact]
        public void TerrainPointJsonConverter_Serialize()
        {
            Assert.Equal("[12,34]", JsonSerializer.Serialize(new TerrainPoint(12,34)));
            Assert.Equal("[12.3456,34.5678]", JsonSerializer.Serialize(new TerrainPoint(12.3456f, 34.5678f)));
        }

        [Fact]
        public void TerrainPointJsonConverter_Deserialize()
        {
            Assert.Equal(new TerrainPoint(12, 34), JsonSerializer.Deserialize<TerrainPoint>("[12,34]"));
            Assert.Equal(new TerrainPoint(12.3456f, 34.5678f), JsonSerializer.Deserialize<TerrainPoint>("[12.3456,34.5678]"));

            Assert.Equal(new TerrainPoint(12, 34), JsonSerializer.Deserialize<TerrainPoint>("{\"x\":12, \"y\":34}"));
            Assert.Equal(new TerrainPoint(12.3456f, 34.5678f), JsonSerializer.Deserialize<TerrainPoint>("{\"x\":12.3456, \"y\":34.5678}"));

            Assert.Equal(new[] { new TerrainPoint(12, 34), new TerrainPoint(56, 89) }, JsonSerializer.Deserialize<TerrainPoint[]>("[[12,34],[56,89]]"));
        }
    }
}
