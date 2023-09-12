using System.Windows;
using System.Windows.Media;
using GameRealisticMap.ManMade.Railways;

namespace GameRealisticMap.Studio.Controls
{
    internal static class GrmMapStyle
    {
        internal static readonly SolidColorBrush OceanBrush = new SolidColorBrush(Color.FromArgb(128, 65, 105, 225));
        internal static readonly SolidColorBrush ForestBrush = new SolidColorBrush(Color.FromArgb(128, 34, 139, 34));
        internal static readonly SolidColorBrush RoadBrush = new SolidColorBrush(Color.FromArgb(192, 75, 0, 130));
        internal static readonly SolidColorBrush BuildingBrush = new SolidColorBrush(Color.FromArgb(128, 139, 69, 19));
        internal static readonly SolidColorBrush ScrubsBrush = new SolidColorBrush(Color.FromArgb(128, 244, 164, 96));
        internal static readonly Pen RailwayPen = new Pen(new SolidColorBrush(Color.FromArgb(204, 0, 0, 0)), Railway.RailwayWidth);

        internal static readonly Pen FalsePen = new Pen(new SolidColorBrush(Colors.Red), 2);
        internal static readonly SolidColorBrush FalseFill = new SolidColorBrush(Colors.White);

        internal static readonly Pen TruePen = new Pen(new SolidColorBrush(Colors.Green), 2);
        internal static readonly SolidColorBrush TrueFill = new SolidColorBrush(Colors.Black);



        internal static Pen? GetAdditionalPen(string name)
        {
            switch (name)
            {
                case "Sidewalks":
                    return new Pen(new SolidColorBrush(Color.FromArgb(204, 0, 0, 0)), 1)
                    {
                        DashStyle = new DashStyle(new[] { 2d, 2d }, 0)
                    };

                case "Fences":
                    return new Pen(new SolidColorBrush(Colors.Red), 1);

                case "TreeRows":
                    return new Pen(ForestBrush, 5) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };

                case "Contours":
                    return new Pen(new SolidColorBrush(Color.FromArgb(64,0,0,0)), 1);
            }
            return null;
        }

        internal static Brush? GetAdditionalBrush(string name)
        {
            switch (name)
            {
                case "DefaultAreas":
                    return CreateStripesFilling(Color.FromArgb(51, 144, 238, 144));

                case "Meadows":
                case "Orchard":
                case "Vineyard":
                    return new SolidColorBrush(Color.FromArgb(89, 173, 255, 47));

                case "Farmlands":
                    return new SolidColorBrush(Color.FromArgb(192, 240, 230, 140));

                case "Trees":
                    return ForestBrush;

                case "Rocks":
                    return new SolidColorBrush(Color.FromArgb(128, 0, 0, 0));

                case "ForestRadial":
                    return new SolidColorBrush(Color.FromArgb(64, 34, 139, 34));

                case "ForestEdge":
                    return new SolidColorBrush(Color.FromArgb(192, 34, 139, 34));

                case "ScrubRadial":
                    return new SolidColorBrush(Color.FromArgb(64, 244, 164, 96));

                case "SandSurfaces":
                    return new SolidColorBrush(Color.FromArgb(128, 218, 165, 32));

                case "Grass":
                    return new SolidColorBrush(Color.FromArgb(62, 144, 238, 144));

                case "Coastline":
                    return CreateStripesFilling(Color.FromArgb(51, 218, 165, 32));

                case "WatercourseRadial":
                    return ForestBrush;
            }
            if (name.StartsWith("Default"))
            {
                return CreateStripesFilling(Color.FromArgb(51, 0, 0, 0));
            }
            return new SolidColorBrush(Color.FromArgb(204, 0, 0, 0));
        }

        private static Brush CreateStripesFilling(Color color)
        {
            return new LinearGradientBrush(
                new GradientStopCollection(new[] {
                            new GradientStop(color, 0), new GradientStop(color, 0.5),
                            new GradientStop(Colors.Transparent, 0.5), new GradientStop(Colors.Transparent, 1) }))
            {
                SpreadMethod = GradientSpreadMethod.Repeat,
                MappingMode = BrushMappingMode.Absolute,
                StartPoint = new Point(0, 0),
                EndPoint = new Point(4, 4)
            };
        }
    }
}
