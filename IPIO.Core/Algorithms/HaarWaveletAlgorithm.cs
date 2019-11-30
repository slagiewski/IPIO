using Accord.Math.Wavelets;
using IPIO.Core.Extensions;
using IPIO.Core.Interfaces;
using IPIO.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using Haar = Accord.Math.Wavelets.Haar;

namespace IPIO.Core.Algorithms
{
    public class HaarWaveletAlgorithm : IStringEmbeddingAlgorithm
    {
        readonly int MAX_DEPTH = 4;
        public Task<Bitmap> EmbedAsync(Bitmap bitmap, string message)
        {
            var messageWithEndChar = message + '\0';
            var charBinaryIndex = 0;
            var charIndex = 0;
            int charValue = messageWithEndChar[charIndex];

            bool MessageAlreadyEmbedded() => charIndex == messageWithEndChar.Length;

            return Task.Run(() =>
            {
                var haar = new Haar(MAX_DEPTH);
                var blueOfPixels = new List<double>();
                bitmap.ForEach(pixel => { blueOfPixels.Add(pixel.B); });
                var arrayBlueOfPixels = blueOfPixels.ToArray();
                haar.Forward(arrayBlueOfPixels);
                for (var i = 0; !MessageAlreadyEmbedded() && i < arrayBlueOfPixels.Length; i++)
                {
                    arrayBlueOfPixels[i] = GetByteWithModifiedLsb((int)arrayBlueOfPixels[i], GetLsb(charValue));
                    if (IsEndOfChar(charBinaryIndex))
                    {
                        charBinaryIndex = 0;
                        charIndex++;
                        if (!MessageAlreadyEmbedded())
                        {
                            charValue = messageWithEndChar[charIndex];
                        }
                    }
                    else
                    {
                        charBinaryIndex++;
                        charValue = ExposeNextBit(charValue);
                    }

                }
                haar.Backward(arrayBlueOfPixels);
                var index = 0;
                return bitmap.Select(pixel =>
                {
                    return new Pixel(pixel.R, pixel.G, (int)arrayBlueOfPixels[index++], pixel.Alpha, pixel.Row, pixel.Column);
                });
            });
        }

        public Task<string> RetrieveAsync(Bitmap bitmap)
        {
            return Task.Run(() =>
            {
                var charBinaryIndex = 0;
                var charValue = 0;
                var messageSB = new StringBuilder();
                var messageRead = false;
                bool IsMessageEndChar() => charValue == 0;


                bitmap.ForEach(pixel =>
                {
                    if (messageRead) return;
                    if (IsEndOfChar(charBinaryIndex))
                    {
                        if (IsMessageEndChar())
                        {
                            messageRead = true;
                            return;
                        }

                        messageSB.Append((char)charValue);
                        charBinaryIndex = 0;
                        charValue = 0;
                    }
                    var charBit = GetLsb(pixel.B);
                    charValue += charBit * (int)Math.Pow(2, charBinaryIndex++);
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

        private static bool IsEndOfChar(int charBinaryIndex) => charBinaryIndex == 7;
    }
}
