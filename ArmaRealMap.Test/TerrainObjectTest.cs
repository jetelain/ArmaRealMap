using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Geometries;
using ArmaRealMap.Libraries;
using Xunit;

namespace ArmaRealMap.Test
{
    public class TerrainObjectTest
    {

       
        [Fact]
        public void TerrainObject_ToTerrainBuilderCSV_ShiftedCenter()
        {
            var x = new TerrainObject(new SingleObjetInfos() { CX = 0, CY = 5f, CZ = 0f, Name = "Model" }, new TerrainPoint(15f, 15f), 0f);
            Assert.Equal(@"""Model"";200015.000;20.000;-0.000;0.0;0.0;1;0.000;", x.ToTerrainBuilderCSV());

            x = new TerrainObject(new SingleObjetInfos() { CX = 0, CY = 5f, CZ = 0f, Name = "Model" }, new TerrainPoint(15f, 15f), 90f);
            Assert.Equal(@"""Model"";200010.000;15.000;-90.000;0.0;0.0;1;0.000;", x.ToTerrainBuilderCSV());

            x = new TerrainObject(new SingleObjetInfos() { CX = 0, CY = 5f, CZ = 0f, Name = "Model" }, new TerrainPoint(15f, 15f), 180f);
            Assert.Equal(@"""Model"";200015.000;10.000;-180.000;0.0;0.0;1;0.000;", x.ToTerrainBuilderCSV());

            x = new TerrainObject(new SingleObjetInfos() { CX = 0, CY = 5f, CZ = 0f, Name = "Model" }, new TerrainPoint(15f, 15f), 270f);
            Assert.Equal(@"""Model"";200020.000;15.000;-270.000;0.0;0.0;1;0.000;", x.ToTerrainBuilderCSV());

            x = new TerrainObject(new SingleObjetInfos() { CX = 0, CY = 0f, CZ = 0f, Name = "Model" }, new TerrainPoint(15f, 15f), 270f);
            Assert.Equal(@"""Model"";200015.000;15.000;-270.000;0.0;0.0;1;0.000;", x.ToTerrainBuilderCSV());

            x = new TerrainObject(new SingleObjetInfos() { CX = 0, CY = 0f, CZ = 0f, Name = "Model" }, new TerrainPoint(15f, 15f), 0f);
            Assert.Equal(@"""Model"";200015.000;15.000;-0.000;0.0;0.0;1;0.000;", x.ToTerrainBuilderCSV());
        }
    }
}
