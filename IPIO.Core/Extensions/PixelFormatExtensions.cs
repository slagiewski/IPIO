using System.Drawing.Imaging;

namespace IPIO.Core.Extensions
{
    public static class PixelFormatExtensions
    {
        public static bool IsArgb(this PixelFormat pixelFormat) =>
            pixelFormat == PixelFormat.Format32bppArgb;
    }
}