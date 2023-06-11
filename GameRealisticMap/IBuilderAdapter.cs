using GameRealisticMap.IO;

namespace GameRealisticMap
{
    internal interface IBuilderAdapter
    {
        IDataBuilder<object> Builder { get; }

        TResult Accept<TResult>(IDataBuilderVisitor<TResult> visitor);

        object Get(IContext ctx);
    }
}