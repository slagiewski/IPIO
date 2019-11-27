using IPIO.Core.Extensions;
using IPIO.Core.Interfaces;
using IPIO.Core.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace IPIO.Core.Algorithms
{
    public class DctAlgorithm : IStringEmbeddingAlgorithm
    {
        int _widht = 0;
        int _height = 0;

        public async Task<Bitmap> EmbedAsync(Bitmap bmp, string message)
        {
            _widht = bmp.Width;
            _height = bmp.Height;

            return await Task.Run(() =>
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
                        return;
                    }

                    block.EmbedChar(GetLsb(charValue));
                    charValue = ExposeNextBit(charValue);

                    BitmapExtensions.CopyBlockIntoPixelsArray(block, bmp.Width, newPixels);

                    charBinaryIndex++;

                    if (IsFinalBitOfWord(charBinaryIndex))
                    {
                        charIndex++;
                        if (!MessageAlreadyEmbedded())
                        {
                            charValue = messageWithEndChar[charIndex];
                        }
                    }                    
                });

                foreach (var pixel in newPixels)
                {
                    Pixel.SetByteArrayValue(modifiedRgbValues, pixel, bitmapData.Stride);
                }

                bmp.UnlockBits(bitmapData);

                var bitmap = modifiedRgbValues.ToBitmap(bitmapData.Width, bitmapData.Height, bmp.PixelFormat);

                return bitmap;               
            });
        }

        public async Task<string> RetrieveAsync(Bitmap bmp)
        {
            return await Task.Run(() =>
            {
                var rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                var bitmapData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

                var blocks = bitmapData.IntoBlocks();

                var charBinaryIndex = 0;
                var charValue = 0;
                var messageSB = new StringBuilder();
                var messageRead = false;

                bool IsMessageEndChar() => charValue == 0;

                blocks.Content.ForEach(block =>
                {
                    if (messageRead)
                    {
                        return;
                    }

                    var charBit = block.GetCharBit();
                    charValue += charBit * (int)Math.Pow(2, charBinaryIndex++);
                    if (IsMessageEndChar())
                    {
                        messageRead = true;
                        return;
                    }

                    if (IsFinalBitOfWord(charBinaryIndex))
                    {
                        charBinaryIndex = 0;
                        charValue = 0;
                    }
                    

                });
                return messageSB.ToString();
            });
        }

        private static int GetLsb(int value) => value % 2;
        private static bool IsFinalBitOfWord(int charBinaryIndex) => charBinaryIndex == 8;
        private static int ExposeNextBit(int value) => value / 2;
        private static int GetByteWithModifiedLsb(int value, int newLsb)
        {
            if (newLsb == 1)
            {
                return value | newLsb;
            }
            else
            {
                return value & ~1;
            }
        }

        private static int GetBytesCount(BitmapData bitmapData)
        {
            var distanceBetweenVerticalPixels = bitmapData.Stride;
            return distanceBetweenVerticalPixels * bitmapData.Height;
        }

     
    }
}
