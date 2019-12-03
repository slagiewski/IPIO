﻿using IPIO.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace IPIO.Core.Extensions
{
    public static class BitmapExtensions
    {
        public static Bitmap Select(this Bitmap bmp, Func<Pixel, Pixel> expression)
        {
            var bitmapData = LockBitmap(bmp);

            var modifiedRgbValues = new byte[bitmapData.GetBytesCount()];

            Iterate(bitmapData, pixel =>
            {
                var modifiedPixel = expression(pixel);
                Pixel.SetByteArrayValue(modifiedRgbValues, modifiedPixel, bitmapData.Stride);
            });
            
            bmp.UnlockBits(bitmapData);

            var bitmap = modifiedRgbValues.ToBitmap(bitmapData.Width, bitmapData.Height, bmp.PixelFormat);

            return bitmap;
        }

        public static void ForEach(this Bitmap bmp, Action<Pixel> expression)
        {
            var bitmapData = LockBitmap(bmp);

            Iterate(bitmapData, pixel => expression(pixel));

            bmp.UnlockBits(bitmapData);
        }

        public static byte[] ToBytes(this Bitmap bmp)
        {
            var bitmapData = LockBitmap(bmp);

            var rgbValues = new byte[bitmapData.GetBytesCount()];

            Iterate(bitmapData, pixel =>
            {
                Pixel.SetByteArrayValue(rgbValues, pixel, bitmapData.Stride);
            });

            bmp.UnlockBits(bitmapData);

            return rgbValues;
        }

        public static Pixel[] ToPixels(this BitmapData bitmapData)
        {
            var pixels = new List<Pixel>(bitmapData.Width * bitmapData.Height);

            Iterate(
                bitmapData, 
                pixel =>
                {
                    pixels.Add(pixel);
                });

            return pixels
                .Where(p => !p.IsEmpty)
                .OrderBy(p => p.Row)
                .ThenBy(p => p.Column)
                .ToArray();
        }

        public static void Iterate(BitmapData bitmapData, Action<Pixel> expression)
        {
            var bytesCount = bitmapData.GetBytesCount();
            var rgbValues = new byte[bytesCount];

            CopyRGBValuesIntoArray(bitmapData, bytesCount, rgbValues);

            for (var row = 0; row < bitmapData.Height; row++)
            {
                for (var column = 0; column < bitmapData.Width; column++)
                {
                    expression(Pixel.FromByteArray(rgbValues, bitmapData.Stride, row, column, bitmapData.PixelFormat));
                }
            }
        }

        private static int GetBytesCount(this BitmapData bitmapData)
        {
            var distanceBetweenVerticalPixels = bitmapData.Stride;
            return distanceBetweenVerticalPixels * bitmapData.Height;
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
    }
}