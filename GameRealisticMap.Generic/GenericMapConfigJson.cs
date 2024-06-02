namespace GameRealisticMap.Generic
{
    public class GenericMapConfigJson
    {
        public double Resolution { get; set; } = 1;

        public float GridCellSize { get; set; } = 5;

        public int GridSize { get; set; } = 1024;

        public string? SouthWest { get; set; }

        public string? Center { get; set; }

        public string? ExportProfileFile { get; set; }

        public string? TargetDirectory { get; set; }

        public GenericMapConfig ToMapConfig()
        {
            return new GenericMapConfig(this);
        }
    }
}
