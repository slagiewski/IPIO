namespace IPIO.Core.Models
{
    public struct Pixel
    {
        public readonly byte R { get; }
        public readonly byte G { get; }
        public readonly byte B { get; }
        public readonly byte? Alpha { get; }

        public Pixel(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
            Alpha = null;
        }

        public Pixel(byte r, byte g, byte b, byte? a)
        {
            R = r;
            G = g;
            B = b;
            Alpha = a;
        }

        public Pixel(int r, int g, int b, int? a)
        {
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
            Alpha = (byte?)a;
        }
    }
}