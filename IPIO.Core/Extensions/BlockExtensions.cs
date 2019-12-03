using IPIO.Core.Models;
using IPIO.Core.Utils;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;

namespace IPIO.Core.Extensions
{
    public static class BlockExtensions
    {
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
                            GetBlock(blockWidth, blockHeight, bitmapData.Width, pixels, col, row)
                            );
                }
            }

            return new Blocks(blocks, bitmapData.Width, bitmapData.Height, blockWidth, blockHeight);
        }

        public static Block GetBlock(int blockWidth, int blockHeight, int bitmapWidth, Pixel[] pixels, int col, int row)
        {
            var pixelsInBlock = new List<Pixel>(blockWidth * blockHeight);

            var colOffset = col * blockWidth;
            var rowOffset = row * blockHeight * bitmapWidth;

            for (var blockRow = 0; blockRow < blockHeight; blockRow++)
            {
                for (var blockCol = 0; blockCol < blockWidth; blockCol++)
                {
                    pixelsInBlock.Add(pixels[colOffset + blockCol +
                                             rowOffset + (blockRow * bitmapWidth)]);
                }
            }

            return new Block(pixelsInBlock, blockWidth, blockHeight, col, row);
        }

        public static void CopyBlockIntoPixelsArray(Block block, int bitmapWidth, Pixel[] pixels)
        {
            var colOffset = block.Column * block.BlockWidth;
            var rowOffset = block.Row * block.BlockHeight * bitmapWidth;

            for (var rowInsideBlock = 0; rowInsideBlock < block.BlockHeight; rowInsideBlock++)
            {
                for (var colInsideBlock = 0; colInsideBlock < block.BlockWidth; colInsideBlock++)
                {
                    var newListIndex = colOffset + colInsideBlock +
                                       rowOffset + (rowInsideBlock * bitmapWidth);

                    var valueFromBlock = block.Pixels[colInsideBlock + rowInsideBlock * block.BlockWidth];

                    pixels[newListIndex] = valueFromBlock;
                }
            }
        }

        public static Block EmbedChar(this Block block, byte newValue)
        {
            var blueDct = block.GetDctOfBlueColor();
            blueDct[0] = GetTransformedValue(blueDct[1], newValue);

            var newBlockOfBlueColor = blueDct.FromBlueDctToBlock(block.BlockWidth, block.BlockHeight);

            for (int i = 0; i < block.Pixels.Count; i++)
            {
                var oldPixel = block.Pixels[i];
                block.Pixels[i] = new Pixel(oldPixel.R, oldPixel.G, (byte)newBlockOfBlueColor[i], oldPixel.Row, oldPixel.Column);
            }

            return block;
        }

        public static int GetChar(this Block transformedBlock, Block originalBlock)
        {
            var transformedBlockDct = transformedBlock.GetDctOfBlueColor();
            var originalBlockDct = originalBlock.GetDctOfBlueColor();

            var transformedValue = transformedBlockDct[0];
            var originalValue = originalBlockDct[0];

            return (int)Math.Round(RetrieveTransformedValue(transformedValue, originalValue));
        }

        private static double[] GetDctOfBlueColor(this Block block)
        {
            var dct = new double[block.BlockWidth * block.BlockHeight];

            for (int p = 0; p < block.BlockHeight; p++)
            {
                for (int q = 0; q < block.BlockWidth; q++)
                {
                    var ap = DCTTools.CalculateAP(p, block.BlockWidth);
                    var aq = DCTTools.CalculateAQ(q, block.BlockHeight);

                    var sum = 0.0;
                    for (var k = 0; k < block.BlockHeight; k++)
                    {
                        for (var l = 0; l < block.BlockWidth; l++)
                        {
                            sum += block.Pixels[l + k * block.BlockWidth].B *
                                   Math.Cos((Math.PI * (2 * l + 1) * q) / (2 * block.BlockWidth)) *
                                   Math.Cos((Math.PI * (2 * k + 1) * p) / (2 * block.BlockHeight));
                        }
                    }

                    dct[q + block.BlockWidth * p] = ap * aq * sum;
                }
            }

            return dct;
        }

        private static double[] FromBlueDctToBlock(this double[] dctBLock, int blockWidth, int blockHeight)
        {
            var blueOfPixels = new double[dctBLock.Length];

            for (int p = 0; p < blockHeight; p++)
            {
                for (int q = 0; q < blockWidth; q++)
                {
                    var blueOfPixel = 0.0;
                    for (var k = 0; k < blockWidth; k++)
                    {
                        for (var l = 0; l < blockHeight; l++)
                        {
                            var ak = DCTTools.CalculateAP(k, blockWidth);
                            var al = DCTTools.CalculateAQ(l, blockHeight);

                            blueOfPixel += ak * al * dctBLock[l + blockWidth * k] * 
                                   Math.Cos(((Math.PI * (2d * p + 1d) * k)) / (2 * blockWidth)) *
                                   Math.Cos(((Math.PI * (2d * q + 1d) * l)) / (2 * blockHeight));

                        }
                    }

                    blueOfPixels[p * blockWidth + q] = blueOfPixel;
                }
            }

            return blueOfPixels;
        }

        private static double GetTransformedValue(double originalValue, double watermarkValue) =>
            originalValue + 0.1 * watermarkValue;

        private static double RetrieveTransformedValue(double encodedValue, double originalValue)
        {
            return (encodedValue - originalValue) / 0.1;
        }
    }
}

