namespace GameRealisticMap.Studio.Modules.CompositionTool.Views
{
    internal static class CanvasGrid
    {
        public static double Scale { get; } = 20;

        public static double Size { get; } = Scale * 60; // 60 x 60 meters preview

        public static double HalfSize { get; } = Size / 2;
    }
}
