﻿namespace GameRealisticMap
{
    public interface IBuidersCatalog
    {
        void Register<TData>(IDataBuilder<TData> builder)
            where TData : class;

        IDataBuilder<TData> Get<TData>() 
            where TData : class;
    }
}