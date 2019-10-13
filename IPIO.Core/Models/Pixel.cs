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

    }
}