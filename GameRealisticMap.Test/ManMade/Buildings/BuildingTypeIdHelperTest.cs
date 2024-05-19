using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.ManMade.Buildings;
using OsmSharp.Tags;

namespace GameRealisticMap.Test.ManMade.Buildings
{
    public class BuildingTypeIdHelperTest
    {

        [Fact]
        public void FromOSM()
        {
            Assert.Equal(BuildingTypeId.Agricultural, BuildingTypeIdHelper.FromOSM(new TagsCollection() { new Tag("building", "cowshed") } ));
            Assert.Equal(BuildingTypeId.Residential, BuildingTypeIdHelper.FromOSM(new TagsCollection() { new Tag("building", "terrace") }));
            Assert.Null(BuildingTypeIdHelper.FromOSM(new TagsCollection() { new Tag("building", "unknown-value") }));
            Assert.Null(BuildingTypeIdHelper.FromOSM(new TagsCollection()));
        }
    }
}
