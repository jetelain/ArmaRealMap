namespace GameRealisticMap.Studio.Modules.AssetConfigEditor.Views.Filling
{
    internal class PreviewGrid
    {
        public static double Scale { get; } = 5;

        public static double SizeInMeters { get; } = 200;

        public static double Size { get; } = Scale * SizeInMeters;

        public static double HalfSize { get; } = Size / 2;
    }
}
