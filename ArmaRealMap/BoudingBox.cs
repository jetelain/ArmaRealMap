namespace ArmaRealMap
{
    internal class BoudingBox
    {

        public BoudingBox(double cx, double cy, double cw, double ch, double ca)
        {
            CenterX = cx;
            CenterY = cy;
            Width = cw;
            Height = ch;
            Angle = ca;
        }

        public double CenterX { get; }
        public double CenterY { get; }
        public double Width { get; }
        public double Height { get; }
        public double Angle { get; }
    }
}