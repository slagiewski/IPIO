using IPIO.Core.Extensions;
using System.Drawing.Imaging;

namespace IPIO.Core.Models
{
    public struct Pixel
    {
        public readonly byte R { get; }
        public readonly byte G { get; }
        public readonly byte B { get; }
        public readonly byte? Alpha { get; }
        public readonly int Row { get; }
        public readonly int Column { get; }

        public Pixel(byte r, byte g, byte b, int row, int column)
        {
            R = r;
            G = g;
            B = b;
            Alpha = null;
            Row = row;
            Column = column;
        }

        public Pixel(byte r, byte g, byte b, byte? a, int row, int column)
        {
            R = r;
            G = g;
            B = b;
            Alpha = a;
            Row = row;
            Column = column;
        }

        public Pixel(int r, int g, int b, int? a, int row, int column)
        {
            R = (byte)r;
            G = (byte)g;
            B = (byte)b;
            Alpha = (byte?)a;
            Row = row;
            Column = column;
        }

        #region Bytes Array Helpers
        public static Pixel FromByteArray(byte[] rgbValues, int stride, int row, int column, PixelFormat pixelFormat)
        {
            if (pixelFormat.IsArgb())
            {
                return new Pixel(
                   r: rgbValues[row * stride + column * 4 + 2],
                   g: rgbValues[row * stride + column * 4 + 1],
                   b: rgbValues[row * stride + column * 4],
                   a: rgbValues[row * stride + column * 4 + 3],
                   row,
                   column
               );
            }
            else
            {
                return new Pixel(
                   r: rgbValues[row * stride + column * 3 + 2],
                   g: rgbValues[row * stride + column * 3 + 1],
                   b: rgbValues[row * stride + column * 3],
                   row,
                   column
               );
            }
        }

        public static void SetByteArrayValue(byte[] rgbValues, Pixel pixel, int stride)
        {
            if (pixel.Alpha.HasValue)
            {
                rgbValues[pixel.Row * stride + pixel.Column * 4] = pixel.B;
                rgbValues[pixel.Row * stride + pixel.Column * 4 + 1] = pixel.G;
                rgbValues[pixel.Row * stride + pixel.Column * 4 + 2] = pixel.R;
                rgbValues[pixel.Row * stride + pixel.Column * 4 + 3] = pixel.Alpha.Value;
            }
            else
            {
                rgbValues[pixel.Row * stride + pixel.Column * 3] = pixel.B;
                rgbValues[pixel.Row * stride + pixel.Column * 3 + 1] = pixel.G;
                rgbValues[pixel.Row * stride + pixel.Column * 3 + 2] = pixel.R;
            }

        }
        #endregion
    }
}