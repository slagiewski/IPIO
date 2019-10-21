using IPIO.Core.Extensions;
using IPIO.Core.Interfaces;
using IPIO.Core.Models;
using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace IPIO.Core.Algorithms
{
    public class LsbAlgorithm : IStringEmbeddingAlgorithm
    {
        public async Task<Bitmap> EmbedAsync(Bitmap bitmap, string message)
        {
            return await Task.Run(() =>
            {
                var messageWithEndChar = message + '\0';
                var charBinaryIndex = 0;
                var charIndex = 0;
                int charValue = messageWithEndChar[charIndex];

                bool MessageAlreadyEmbedded() => charIndex == messageWithEndChar.Length;

                return bitmap.Select(pixel =>
                {
                    if (MessageAlreadyEmbedded())
                    {
                        return pixel;
                    }

                    if (IsThirdPixel(charBinaryIndex))
                    {
                        var r = GetByteWithModifiedLsb(pixel.R, GetLsb(charValue));
                        charValue = ExposeNextBit(charValue);

                        var g = GetByteWithModifiedLsb(pixel.G, GetLsb(charValue));

                        charBinaryIndex = 0;
                        charIndex++;

                        if (!MessageAlreadyEmbedded())
                        {
                            charValue = messageWithEndChar[charIndex];
                        }
                        
                        return new Pixel(r, g, pixel.B, pixel.Alpha, pixel.Row, pixel.Column);
                    }
                    else
                    {
                        var r = GetByteWithModifiedLsb(pixel.R, GetLsb(charValue));
                        charValue = ExposeNextBit(charValue);

                        var g = GetByteWithModifiedLsb(pixel.G, GetLsb(charValue));
                        charValue = ExposeNextBit(charValue);

                        var b = GetByteWithModifiedLsb(pixel.B, GetLsb(charValue));
                        charValue = ExposeNextBit(charValue);

                        charBinaryIndex += 3;

                        return new Pixel(r, g, b, pixel.Alpha, pixel.Row, pixel.Column);
                    }
                });
            });                
        }

        public async Task<string> RetrieveAsync(Bitmap bitmap)
        {
            return await Task.Run(() =>
            {
                var charBinaryIndex = 0;
                var charValue = 0;
                var messageSB = new StringBuilder();
                var messageRead = false;

                bool IsMessageEndChar() => charValue == 0;

                bitmap.ForEach(pixel =>
                {
                    if (messageRead)
                    {
                        return;
                    }

                    if (IsThirdPixel(charBinaryIndex))
                    {
                        var charBit = GetLsb(pixel.R);
                        charValue += charBit * (int)Math.Pow(2, charBinaryIndex++);

                        charBit = GetLsb(pixel.G);
                        charValue += charBit * (int)Math.Pow(2, charBinaryIndex);

                        if (IsMessageEndChar())
                        {
                            messageRead = true;
                            return;
                        }

                        messageSB.Append((char)charValue);
                        charBinaryIndex = 0;
                        charValue = 0;
                    }
                    else
                    {
                        var charBit = GetLsb(pixel.R);
                        charValue += charBit * (int)Math.Pow(2, charBinaryIndex++);

                        charBit = GetLsb(pixel.G);
                        charValue += charBit * (int)Math.Pow(2, charBinaryIndex++);

                        charBit = GetLsb(pixel.B);
                        charValue += charBit * (int)Math.Pow(2, charBinaryIndex++);
                    }
                });

                return messageSB.ToString();
            });
        }

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

        private static int GetLsb(int value) => value % 2;

        private static int ExposeNextBit(int value) => value / 2;

        private static bool IsThirdPixel(int charBinaryIndex) => charBinaryIndex == 6;
    }
}
