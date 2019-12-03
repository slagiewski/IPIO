using IPIO.Core.Extensions;
using IPIO.Core.Interfaces;
using IPIO.Core.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;

namespace IPIO.Core.Algorithms
{
    public class DctAlgorithm : IStringEmbeddingAlgorithm
    {
        public async Task<Bitmap> EmbedAsync(Bitmap bmp, string message)
        {
            return await Task.Run(async () =>
            {
                var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                var bitmapData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

                var modifiedRgbValues = new byte[GetBytesCount(bitmapData)];

                var blocks = bitmapData.IntoBlocks();

                var messageWithEndChar = message + '\0';
                var charBinaryIndex = 0;
                var charIndex = 0;
                int charValue = messageWithEndChar[charIndex];
                bool MessageAlreadyEmbedded() => charIndex == messageWithEndChar.Length;

                var newPixels = new Pixel[bmp.Width * bmp.Height];

                blocks.Content.ForEach(block =>
                {
                    if (MessageAlreadyEmbedded())
                    {
                        BlockExtensions.CopyBlockIntoPixelsArray(block, bmp.Width, newPixels);
                        return;
                    }

                    block.EmbedChar((byte)charValue);

                    BlockExtensions.CopyBlockIntoPixelsArray(block, bmp.Width, newPixels);

                    charIndex++;
                    charBinaryIndex = 0;

                    if (!MessageAlreadyEmbedded())
                    {
                        charValue = messageWithEndChar[charIndex];
                    }
                                        
                });

                newPixels.CopyToByteArray(modifiedRgbValues, bitmapData.Stride);

                bmp.UnlockBits(bitmapData);

                var bitmap = modifiedRgbValues.ToBitmap(bitmapData.Width, bitmapData.Height, bmp.PixelFormat);

                var msgFromModifiedImage = await RetrieveAsync(bitmap, bmp);

                if (msgFromModifiedImage != message)
                {
                    throw new Exception("Message was not embedded successfully!");
                }

                return bitmap;               
            });
        }

        public async Task<string> RetrieveAsync(Bitmap bmp)
        {
            throw new NotImplementedException();
        }

        public async Task<string> RetrieveAsync(Bitmap bmp, Bitmap originalBitmap)
        {
            return await Task.Run(() =>
            {
                var watermarkedBitmapData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
                var originalBitmapData = originalBitmap.LockBits(new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height), ImageLockMode.ReadWrite, originalBitmap.PixelFormat);

                var watermarkedBlocks = watermarkedBitmapData.IntoBlocks();
                var originalBlocks = originalBitmapData.IntoBlocks();

                var charBinaryIndex = 0;
                var charValue = 0;
                var messageSB = new StringBuilder();
                var messageRead = false;

                bool IsMessageEndChar() => charValue == 0;

                var index = -1;
                watermarkedBlocks.Content.ForEach(block =>
                {
                    index++;

                    if (messageRead)
                    {
                        return;
                    }

                    charValue = block.GetChar(originalBlocks.Content[index]);
                   
                    if (IsMessageEndChar())
                    {
                        messageRead = true;
                        return;
                    }

                    messageSB.Append((char)charValue);
                    charValue = 0;

                });
                return messageSB.ToString();
            });
        }

        private static int GetLsb(int value) => value % 2;
        private static bool IsFinalBitOfChar(int charBinaryIndex) => charBinaryIndex == 8;
        private static int ExposeNextBit(int value) => value / 2;

        private static int GetBytesCount(BitmapData bitmapData)
        {
            var distanceBetweenVerticalPixels = bitmapData.Stride;
            return distanceBetweenVerticalPixels * bitmapData.Height;
        }

     
    }
}
