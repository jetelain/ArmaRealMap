using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap
{
    public interface IDataBuilder<T> where T : class, ITerrainData
    {
        T Build(IBuildContext context);
    }
}
