namespace GameRealisticMap
{
    public interface IBuidersCatalog
    {
        void Register<TData, TBuidler>(TBuidler builder)
            where TData : class, ITerrainData
            where TBuidler : class, IDataBuilder<TData>;

        IDataBuilder<TData> Get<TData>() 
            where TData : class, ITerrainData;
    }
}