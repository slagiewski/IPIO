using Microsoft.VisualStudio.TestTools.UnitTesting;
using IPIO.Core.Algorithms;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using IPIO.Core.Extensions;
using IPIO.Core.Models;
using System.Linq;

namespace IPIO.Core.Algorithms.Tests
{
    [TestClass()]
    public class DctAlgorithmTests
    {
        public static Bitmap GenerateRandomBitmap()
        {
            // 1. Create a bitmap
            var bitmap = new Bitmap(800, 480, PixelFormat.Format24bppRgb);

            // 2. Get access to the raw bitmap data
            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);

            // 3. Generate RGB noise and write it to the bitmap's buffer.
            // Note that we are assuming that data.Stride == 3 * data.Width for simplicity/brevity here.
            byte[] noise = new byte[data.Width * data.Height * 3];
            new Random().NextBytes(noise);
            Marshal.Copy(noise, 0, data.Scan0, noise.Length);

            bitmap.UnlockBits(data);

            return bitmap;

        }

        [TestMethod()]
        public void EmbedAsyncTest()
        {
            var bmp = GenerateRandomBitmap();

            var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            var bitmapData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

            var modifiedRgbValues = new byte[GetBytesCount(bitmapData)];

            var blocks = bitmapData.IntoBlocks();

            var newPixels = new Pixel[bmp.Width * bmp.Height];

            blocks.Content.ForEach(block =>
            {
                BlockExtensions.CopyBlockIntoPixelsArray(block, bmp.Width, newPixels);
            });

            newPixels.CopyToByteArray(modifiedRgbValues, bitmapData.Stride);

            bmp.UnlockBits(bitmapData);

            var originalBytes = bmp.ToBytes();

            var originalPixels = originalBytes
                .ToPixels(bmp.Width, bmp.Height, bitmapData.Stride, bitmapData.PixelFormat)
                .ToArray();

            var arePixelsDifferent = false;
            for (int i = 0; i < originalPixels.Length; i++)
            {
                if (!originalPixels[i].Equals(newPixels[i]))
                {
                    arePixelsDifferent = true;
                }
            }

            var areBytesDifferent = false;
            for (int i = 0; i < originalBytes.Length; i++)
            {
                if (originalBytes[i] != modifiedRgbValues[i])
                {
                    areBytesDifferent = true;
                }
            }

            Assert.IsFalse(arePixelsDifferent);
            Assert.IsFalse(areBytesDifferent);

        }

        private static int GetBytesCount(BitmapData bitmapData)
        {
            var distanceBetweenVerticalPixels = bitmapData.Stride;
            return distanceBetweenVerticalPixels * bitmapData.Height;
        }
    }
}

namespace IPIO.CoreTests.Algorithms
{
    class DctAlgorithmTests
    {
    }
}
