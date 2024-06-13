using System.Globalization;
using GameRealisticMap.Geometries;
using GameRealisticMap.ManMade.Roads.Libraries;
using GameRealisticMap.Reporting;
using OsmSharp.Tags;

namespace GameRealisticMap.ManMade.Cutlines
{
    internal class CutlinesBuilder : IDataBuilder<CutlinesData>
    {
        private readonly IProgressSystem progress;

        public CutlinesBuilder(IProgressSystem progress)
        {
            this.progress = progress;
        }

        public CutlinesData Build(IBuildContext context)
        {
            var polygons = new List<TerrainPolygon>();

            foreach(var way in context.OsmSource.Ways
                .Where(w => w.Tags != null && w.Tags.GetValue("man_made") == "cutline")
                .ProgressStep(progress, "Interpret"))
            {
                polygons.AddRange(
                    context.OsmSource.Interpret(way)
                    .SelectMany(w => TerrainPath.FromGeometry(w, context.Area.LatLngToTerrainPoint))
                    .Where(p => p.EnveloppeIntersects(context.Area.TerrainBounds))
                    .SelectMany(p => p.ClippedBy(context.Area.TerrainBounds))
                    .SelectMany(p => p.ToTerrainPolygon(GetWidth(way.Tags))));
            }

            return new CutlinesData(polygons);
        }

        private float GetWidth(TagsCollectionBase tags)
        {
            if(tags.TryGetValue("width", out var widthStr) 
                && !string.IsNullOrEmpty(widthStr) 
                && float.TryParse(widthStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var widthValue))
            {
                return widthValue;
            }
            switch(tags.GetValue("cutline"))
            {
                case "section":
                case "border":
                case "loggingmachine":
                case "logway":
                case "pipeline":
                    return 5;

                case "firebreak":
                case "power_line":
                    return 10;

                case "piste":
                    return 15;
            }
            return 10;
        }
    }
}
