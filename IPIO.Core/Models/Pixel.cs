namespace IPIO.Core.Models
{
    public struct Pixel
    {
        public readonly byte R { get; }
        public readonly byte G { get; }
        public readonly byte B { get; }

        public Pixel(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Pixel(int r, int g, int b)
        {
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
        }

    }
}