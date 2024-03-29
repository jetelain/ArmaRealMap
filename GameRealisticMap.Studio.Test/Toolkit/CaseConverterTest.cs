using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameRealisticMap.Studio.Toolkit;
using GameRealisticMap.Studio.UndoRedo;

namespace GameRealisticMap.Studio.Test.Toolkit
{
    public class CaseConverterTest
    {
        [Fact]
        public void ToPascalCase()
        {
            Assert.Equal("AaaBbb", CaseConverter.ToPascalCase("aaa_bbb"));
            Assert.Equal("AaaBbb", CaseConverter.ToPascalCase("AAA_BBB"));
            Assert.Equal("Aaa", CaseConverter.ToPascalCase("AAA"));
            Assert.Equal("AAA", CaseConverter.ToPascalCase("A-A-A"));
        }

    }
}
