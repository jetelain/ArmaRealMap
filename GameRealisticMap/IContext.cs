﻿namespace GameRealisticMap
{
    public interface IContext
    {
        T GetData<T>() where T : class;
    }
}