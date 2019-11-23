using IPIO.Core.Extensions;
using IPIO.Core.Interfaces;
using IPIO.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace IPIO.Core.Algorithms
{
    public class DCTAlgorithm : IStringEmbeddingAlgorithm
    {
        int _widht = 0;
        int _height = 0;

        public async Task<Bitmap> EmbedAsync(Bitmap bitmap, string message)
        {
            _widht = bitmap.Width;
            _height = bitmap.Height;

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
                    if (IsMostRightBottomCornerOfBlock(pixel))
                    {
                        // Taking just most bottom right pixel of each 
                        //block and applying transformation, 
                        //embeding char and undoing transformation.

                        var dtc = TransformDTC(pixel);
                        var modified = GetByteWithModifiedLsb(dtc.coefficient, GetLsb(charValue));
                        dtc.coefficient = modified;
                        var redoDtc = TransformBackFromDTC(dtc);
                        charBinaryIndex++;
                        if (IsFinalBitOfWord(charBinaryIndex))
                        {
                            charIndex++;
                            if (!MessageAlreadyEmbedded())
                            {
                                charValue = messageWithEndChar[charIndex];
                            }
                        }

                        return redoDtc;
                    }
                    else
                    {
                        return pixel;
                    }
                });
            });
        }

        private bool IsMostRightBottomCornerOfBlock(Pixel pixel) => pixel.Column % 8 == 0 && pixel.Row % 8 == 0;
        private DTC TransformDTC(Pixel pixel)
        {
            // Just dont care about the sum -> Simply taking formula and applying on one pixel
            var calculateDTC =
                (int)(Math.Cos((Math.PI * (2 * pixel.Column + 1) * pixel.Column) / (2 * _height)) *
                Math.Cos((Math.PI * (2 * pixel.Row + 1) * pixel.Row) / (2 * _widht)));

            return new DTC
            {
                pixel = pixel,
                coefficient = calculateDTC
            };
        }

        private Pixel TransformBackFromDTC(DTC dtc)
        {
            // I don't know how to transform back this ??
            return dtc.pixel;
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

                    if (IsMostRightBottomCornerOfBlock(pixel))
                    {
                        var dct = TransformDTC(pixel);
                        var charBit = GetLsb(dct.coefficient);
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
    }
}
