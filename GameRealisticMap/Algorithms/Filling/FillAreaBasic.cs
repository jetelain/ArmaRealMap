using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Algorithms.Filling
{
    /// <summary>
    /// Fill area with randomly choosen models
    /// </summary>
    /// <typeparam name="TModelInfo"></typeparam>
    public sealed class FillAreaBasic<TModelInfo> : FillAreaBase<TModelInfo>
    {
        private readonly List<IBasicDefinition<TModelInfo>> basicDefinitions;

        public FillAreaBasic(IProgressSystem progress, List<IBasicDefinition<TModelInfo>> basicDefinitions)
            : base(progress)
        {
            this.basicDefinitions = basicDefinitions;
        }

        internal override AreaFillingBase<TModelInfo> GenerateAreaSelectData(AreaDefinition fillarea)
        {
            return new AreaFillingBasic<TModelInfo>(fillarea, basicDefinitions.GetRandom(fillarea.Random));
        }
    }
}
