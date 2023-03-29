using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GameRealisticMap.Osm;
using GameRealisticMap.Roads;

namespace GameRealisticMap
{
    internal class BuildContext : IBuildContext
    {
        private readonly Dictionary<Type, ITerrainData> datas = new Dictionary<Type, ITerrainData>();
        private readonly Dictionary<Type, object> builders = new Dictionary<Type, object>();

        public void Register<TData, TBuidler>(TBuidler builder)
            where TData : class, ITerrainData
            where TBuidler : class, IDataBuilder<TData>
        {
            builders.Add(typeof(TData), builder);
        }

        public ITerrainArea Area { get; }

        public IOsmDataSource OsmSource { get; }

        public T GetData<T>()
             where T : class, ITerrainData
        {
            if (datas.TryGetValue(typeof(T), out var cachedData))
            {
                return (T)cachedData;
            }

            if (builders.TryGetValue(typeof(T), out var builder))
            {
                var builtData = ((IDataBuilder<T>)builder).Build(this);
                datas[typeof(T)] = builtData;
                return builtData;
            }

            throw new NotImplementedException();
        }
    }
}
