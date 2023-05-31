using GameRealisticMap.Algorithms.Definitions;
using GameRealisticMap.Geometries;
using GameRealisticMap.Reporting;

namespace GameRealisticMap.Algorithms.Filling
{
    /// <summary>
    /// Fill area with randomly choosen models
    /// </summary>
    /// <typeparam name="TModelInfo"></typeparam>
    public sealed class FillAreaBasic<TModelInfo> : FillAreaBase<TModelInfo>
    {
        private readonly IReadOnlyCollection<IBasicDefinition<TModelInfo>> basicDefinitions;

        public FillAreaBasic(IProgressSystem progress, IReadOnlyCollection<IBasicDefinition<TModelInfo>> basicDefinitions)
            : base(progress)
        {
            this.basicDefinitions = basicDefinitions;
        }

        public override int FillPolygons(RadiusPlacedLayer<TModelInfo> objects, List<TerrainPolygon> polygons)
        {
            if (basicDefinitions.Count > 0)
            {
                return base.FillPolygons(objects, polygons);
            }
            return 0;
        }

        internal override AreaFillingBase<TModelInfo> GenerateAreaSelectData(AreaDefinition fillarea)
        {
            return new AreaFillingBasic<TModelInfo>(fillarea, basicDefinitions.GetRandom(fillarea.Random));
        }
    }
}
