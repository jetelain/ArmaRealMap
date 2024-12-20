﻿using System.Diagnostics;
using Pmad.HugeImages.Storage;
using Pmad.ProgressTracking;

namespace GameRealisticMap.IO
{
    internal class ContextReader : IContext, IDataBuilderVisitor<Task>
    {
        private readonly Dictionary<Type, object> cache = new Dictionary<Type, object>();
        private readonly IPackageReader package;
        private readonly IBuidersCatalog catalog;

        public IHugeImageStorage HugeImageStorage => throw new NotImplementedException();

        public ContextReader(IPackageReader package, IBuidersCatalog catalog)
        {
            this.package = package;
            this.catalog = catalog;
        }

        public T GetData<T>(IProgressScope? parentScope = null) where T : class
        {
            if (cache.TryGetValue(typeof(T), out var data))
            {
                return (T)data;
            }
            var result = ContextSerializer.GetSerializer(catalog.Get<T>()).Read(package, this).Result;
            if (result == null)
            {
                throw new InvalidOperationException($"Data type '{typeof(T)}' is not supported.");
            }
            cache[typeof(T)] = result;
            return result;
        }

        public Task<T> GetDataAsync<T>(IProgressScope? parentScope = null) where T : class
        {
            return Task.FromResult(GetData<T>(parentScope));
        }

        public IEnumerable<T> GetOfType<T>() where T : class
        {
            return catalog.GetOfType<T>(this);
        }

        public async Task Visit<TData>(IDataBuilder<TData> builder) where TData : class
        {
            if (!cache.ContainsKey(typeof(TData)))
            {
                var result = await ContextSerializer.GetSerializer(catalog.Get<TData>()).Read(package, this);
                if (result != null)
                {
                    cache[typeof(TData)] = result;
                }
            }
        }

        public async Task ReadAll()
        {
            foreach (var step in catalog.VisitAll(this))
            {
                await step;
            }
        }
    }
}
