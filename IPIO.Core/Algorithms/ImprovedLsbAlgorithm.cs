using IPIO.Core.Extensions;
using IPIO.Core.Interfaces;
using IPIO.Core.Models;
using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace IPIO.Core.Algorithms
{
    public class ImprovedLsbAlgorithm : IStringEmbeddingAlgorithm
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
                        var twoLowBits = ReadAndRotateNextTwoBits(ref charValue);

                        var b = GetByteWithModifiedTwoLowBits(pixel.B, twoLowBits);

                        charBinaryIndex = 0;
                        charIndex++;

                        if (!MessageAlreadyEmbedded())
                        {
                            charValue = messageWithEndChar[charIndex];
                        }

                        return new Pixel(pixel.R, pixel.G, b, pixel.Alpha, pixel.Row, pixel.Column);
                    }
                    else
                    {
                        var r = GetByteWithModifiedLsb(pixel.R, GetLsb(charValue));
                        charValue = ExposeNextBit(charValue);

                        var b = GetByteWithModifiedTwoLowBits(pixel.B, ReadAndRotateNextTwoBits(ref charValue));

                        charBinaryIndex += 3;

                        return new Pixel(r, pixel.G, b, pixel.Alpha, pixel.Row, pixel.Column);
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
                        AddValueFromTwoLowBitsToChar(GetTwoLowBits(pixel.B), ref charBinaryIndex, ref charValue);

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

                        AddValueFromTwoLowBitsToChar(GetTwoLowBits(pixel.B), ref charBinaryIndex, ref charValue);
                    }
                });

                return messageSB.ToString();
            });
        }

        private static int ReadAndRotateNextTwoBits(ref int charValue)
        {
            var lowerLsb = GetLsb(charValue);
            charValue = ExposeNextBit(charValue);
            var higherLsb = GetLsb(charValue);
            charValue = ExposeNextBit(charValue);

            var twoLowBits = lowerLsb + 2 * higherLsb;
            return twoLowBits;
        }

        private static void AddValueFromTwoLowBitsToChar(int twoLowBits, ref int charBinaryIndex, ref int charValue)
        {
            var lowLSB = GetLsb(twoLowBits);
            var highLSB = ExposeNextBit(twoLowBits);

            charValue += (lowLSB * (int)Math.Pow(2, charBinaryIndex++))
                         + (highLSB * (int)Math.Pow(2, charBinaryIndex++));
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

        private static int GetByteWithModifiedTwoLowBits(int value, int twoLowBits) => (value & 0xFC) | twoLowBits;

        private static int GetTwoLowBits(int value) => value & 0x3;

        private static int GetLsb(int value) => value % 2;

        private static int ExposeNextBit(int value) => value / 2;

        private static bool IsThirdPixel(int charBinaryIndex) => charBinaryIndex == 6;
    }
}