namespace GameRealisticMap
{
    public class ImageryOptions : IImageryOptions
    {
        public ImageryOptions(double resolution = 1)
        {
            Resolution = resolution;
        }

        public double Resolution { get; }
    }
}
