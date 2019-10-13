using IPIO.Core.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace IPIO.Core.Extensions
{
    public static class BitmapExtensions
    {
        public static Bitmap Select(this Bitmap bmp, Func<Pixel, Pixel> expression)
        {
            var bitmapData = LockBitmap(bmp);

            DeclareArraysForBitmap(
                bmp,
                bitmapData,
                out int bytesCount,
                out byte[] rgbValues,
                out byte[] modifiedRgbValues
                );

            CopyRGBValuesIntoArray(bitmapData, bytesCount, rgbValues);

            var stride = bitmapData.Stride;

            for (var row = 0; row < bitmapData.Height; row++)
            {
                for (var column = 0; column < bitmapData.Width; column++)
                {
                    var modifiedPixel = expression(GetPixelforPosition(rgbValues, stride, row, column));
                    SetPixelValues(modifiedRgbValues, modifiedPixel, stride, column, row);
                }
            }

            bmp.UnlockBits(bitmapData);

            var bitmap = modifiedRgbValues.ToBitmap(bitmapData.Width, bitmapData.Height, bmp.PixelFormat);

            return bitmap;
        }

        private static void DeclareArraysForBitmap(Bitmap bmp, BitmapData bitmapData, out int bytes, out byte[] rgbValues, out byte[] modifiedRgbValues)
        {
            bytes = bitmapData.Stride * bmp.Height;
            rgbValues = new byte[bytes];
            modifiedRgbValues = new byte[bytes];
        }

        private static BitmapData LockBitmap(Bitmap bmp)
        {
            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bitmapData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);
            return bitmapData;
        }

        private static void CopyRGBValuesIntoArray(BitmapData bitmapData, int bytes, byte[] rgbValues)
        {
            var addressOfTheFirstLine = bitmapData.Scan0;
            Marshal.Copy(addressOfTheFirstLine, rgbValues, 0, bytes);
        }

        private static Pixel GetPixelforPosition(byte[] rgbValues, int stride, int row, int column) =>
            new Pixel(
                    r: rgbValues[row * stride + column * 3],
                    g: rgbValues[row * stride + column * 3 + 1],
                    b: rgbValues[row * stride + column * 3 + 2]
                );
        
        private static void SetPixelValues(byte[] rgbValues, Pixel pixel, int stride, int column, int row)
        {
            rgbValues[row * stride + column * 3] = pixel.R;
            rgbValues[row * stride + column * 3 + 1] = pixel.G;
            rgbValues[row * stride + column * 3 + 2] = pixel.B;
        }

        private static Bitmap ToBitmap(this byte[] bytes, int width, int height, PixelFormat pixelFormat)
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

