using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Conditions;

namespace GameRealisticMap.Test.Conditions
{
    public class TagFilterLanguageTest
    {

        [Fact]
        public void TagFilterLanguage_Invalid()
        {
            try
            {
                TagFilterLanguage.Instance.Parse<IPointConditionContext>("IsResidential && NoSuchProperty && Elevation > 10");
            }
            catch(TagFilterLanguageException ex)
            {
                Assert.Equal("Expression 'IsResidential && NoSuchProperty && Elevation > 10' is invalid: Property 'NoSuchProperty' does not exists.", ex.Message);
                Assert.Equal(17, ex.ErrorSegment.Start);
                Assert.Equal(31, ex.ErrorSegment.End);
            }

        }

    }
}
