using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace IPIO.Core.Extensions
{
    public static class BytesExtensions
    {
        public static Bitmap ToBitmap(this byte[] bytes, int width, int height, PixelFormat pixelFormat)
        {
            using (var stream = new MemoryStream(bytes))
            {
                var bmp = new Bitmap(width, height, pixelFormat);

                var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
                                           ImageLockMode.WriteOnly,
                                           bmp.PixelFormat);

                var firstLineAddress = bmpData.Scan0;
                Marshal.Copy(bytes, 0, firstLineAddress, bytes.Length);

                bmp.UnlockBits(bmpData);

                return bmp;
            }

        }
    }
}
