using IPIO.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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

        public static Blocks IntoBlocks(this BitmapData bitmapData, int blockWidth = 8, int blockHeight = 8)
        {
            var pixels = bitmapData.ToPixels();

            var numberOfColumns = bitmapData.Width / blockWidth;
            var numberOfRows = bitmapData.Height / blockHeight;

            var blocks = new List<Block>(numberOfColumns * numberOfRows);

            for (var row = 0; row < numberOfRows; row++)
            {
                for (var col = 0; col < numberOfColumns; col++)
                {
                    blocks.Add(
                            GetBlock(blockWidth, blockHeight, bitmapData, pixels, col, row)
                            );
                }
            }

            return new Blocks(blocks, bitmapData.Width, bitmapData.Height, blockWidth, blockHeight);
        }

        private static Block GetBlock(int blockWidth, int blockHeight, BitmapData bitmapData, Pixel[] pixels, int col, int row)
        {
            var pixelsInBlock = new List<Pixel>(blockWidth * blockHeight);

            var colOffset = col * blockWidth;
            var rowOffset = row * bitmapData.Width;

            for (var blockRow = 0; blockRow < blockHeight; blockRow++)
            {
                for (var blockCol = 0; blockCol < blockWidth; blockCol++)
                {
                    pixelsInBlock.Add(pixels[colOffset + blockCol +
                                             rowOffset + (blockRow * bitmapData.Width)]);
                }
            }

            return new Block(pixelsInBlock, blockWidth, blockHeight, col, row);
        }

        private static Pixel[] ToPixels(this BitmapData bitmapData)
        {
            var pixels = new List<Pixel>(bitmapData.Width * bitmapData.Height);

            Iterate(bitmapData, pixel => pixels.Add(pixel));

            return pixels.ToArray();
        }

        private static void CopyBlockIntoPixelList(Block block, int bitmapWidth, List<Pixel> pixels)
        {
            var colOffset = block.Column * block.BlockWidth;
            var rowOffset = block.Row * bitmapWidth;

            for (var rowInsideBlock = 0; rowInsideBlock < block.BlockWidth; rowInsideBlock++)
            {
                for (var colInsideBlock = 0; colInsideBlock < block.BlockHeight; colInsideBlock++)
                {
                    var newListIndex = colOffset + colInsideBlock +
                                       rowOffset + (colInsideBlock * bitmapWidth);

                    var valueFromBlock = block.Pixels[colInsideBlock + rowInsideBlock * block.BlockWidth];

                    pixels[newListIndex] = valueFromBlock;
                }
            }          
        }

        private static void Iterate(BitmapData bitmapData, Action<Pixel> expression)
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