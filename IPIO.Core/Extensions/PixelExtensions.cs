using IPIO.Core.Models;

namespace IPIO.Core.Extensions
{
    public static class PixelExtensions
    {
        public static void CopyToByteArray(this Pixel[] newPixels, byte[] targetByteArray, int stride)
        {
            foreach (var pixel in newPixels)
            {
                Pixel.SetByteArrayValue(targetByteArray, pixel, stride);
            }
        }
    }
}
