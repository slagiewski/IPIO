using IPIO.Core.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace IPIO.Core.Extensions
{
    public static class BitmapExtensions
    {
        //add bitmap metadata to the func arguments
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
                    var modifiedPixel = expression(GetPixelforPosition(rgbValues, stride, row, column, bitmapData.PixelFormat));
                    SetPixelValues(modifiedRgbValues, modifiedPixel, stride, row, column, bitmapData.PixelFormat);
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

        //TODO: move to separate class
        #region Pixel
        private static Pixel GetPixelforPosition(byte[] rgbValues, int stride, int row, int column, PixelFormat pixelFormat)
        {
            if (pixelFormat.IsArgb())
            {
                return new Pixel(
                   r: rgbValues[row * stride + column * 4 + 2],
                   g: rgbValues[row * stride + column * 4 + 1],
                   b: rgbValues[row * stride + column * 4],
                   a: rgbValues[row * stride + column * 4 + 3]
               );
            }
            else
            {
                return new Pixel(
                   r: rgbValues[row * stride + column * 3 + 2],
                   g: rgbValues[row * stride + column * 3 + 1],
                   b: rgbValues[row * stride + column * 3]
               );
            }
        }
        
        private static void SetPixelValues(byte[] rgbValues, Pixel pixel, int stride, int row, int column, PixelFormat pixelFormat)
        {
            if (pixelFormat.IsArgb())
            {
                rgbValues[row * stride + column * 4] = pixel.B;
                rgbValues[row * stride + column * 4 + 1] = pixel.G;
                rgbValues[row * stride + column * 4 + 2] = pixel.R;
                rgbValues[row * stride + column * 4 + 3] = pixel.Alpha.Value;
            }
            else
            {
                rgbValues[row * stride + column * 3] = pixel.B;
                rgbValues[row * stride + column * 3 + 1] = pixel.G;
                rgbValues[row * stride + column * 3 + 2] = pixel.R;
            }
                
        }
        #endregion
    }
}

