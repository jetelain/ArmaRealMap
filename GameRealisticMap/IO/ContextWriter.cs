using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameRealisticMap.IO
{
    internal class ContextWriter : IDataBuilderVisitor<Task>
    {
        private readonly IPackageWriter package;
        private readonly IContext context;

        public ContextWriter(IPackageWriter package, IContext context)
        {
            this.package = package;
            this.context = context;
        }

        public Task Visit<TData>(IDataBuilder<TData> builder) where TData : class
        {
            return ContextSerializer.GetSerializer(builder).Write(package, context.GetData<TData>());
        }
    }
}
