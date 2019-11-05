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

                    var ycbcr = ConvertToYCbCr(pixel);
                    ycbcr.Y = GetByteWithModifiedLsb((int)ycbcr.Y, GetLsb(charValue));
                    charValue = ExposeNextBit(charValue);
                    charBinaryIndex++;

                    if (IsFinishCharByte(charBinaryIndex))
                    {
                        charBinaryIndex = 0;
                        charIndex++;
                        if (!MessageAlreadyEmbedded())
                        {
                            charValue = messageWithEndChar[charIndex];
                        }
                    }

                    return ConvertToRGB(ycbcr);
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

                    var ycbcr = ConvertToYCbCr(pixel);
                    var charBit = GetLsb((int)ycbcr.Y);
                    charValue += charBit * (int)Math.Pow(2, charBinaryIndex++);
                    if (IsMessageEndChar())
                    {
                        messageRead = true;
                        return;
                    }
                    if (IsFinishCharByte(charBinaryIndex))
                    {
                        messageSB.Append((char)charValue);
                        charBinaryIndex = 0;
                        charValue = 0;
                    }
                });

                return messageSB.ToString();
            });
        }

        private YCbCr ConvertToYCbCr(Pixel pixel)
        {
            //according to https://www.programmingalgorithms.com/algorithm/rgb-to-ycbcr/
            float y = (float)(0.2989 * pixel.R + 0.5866 * pixel.G + 0.1145 * pixel.B);
            float cb = (float)(-0.1687 * pixel.R - 0.3313 * pixel.G + 0.5000 * pixel.B);
            float cr = (float)(0.5000 * pixel.R - 0.4184 * pixel.G - 0.0816 * pixel.B);

            return new YCbCr(y, cb, cr, pixel.Row, pixel.Column);
        }

        private Pixel ConvertToRGB(YCbCr ycbcr)
        {
            //according to https://www.programmingalgorithms.com/algorithm/ycbcr-to-rgb/
            float r = Math.Max(0.0f, Math.Min(1.0f, (float)(ycbcr.Y + 0.0000 * ycbcr.Cb + 1.4022 * ycbcr.Cr)));
            float g = Math.Max(0.0f, Math.Min(1.0f, (float)(ycbcr.Y - 0.3456 * ycbcr.Cb - 0.7145 * ycbcr.Cr)));
            float b = Math.Max(0.0f, Math.Min(1.0f, (float)(ycbcr.Y + 1.7710 * ycbcr.Cb + 0.0000 * ycbcr.Cr)));

            return new Pixel((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), ycbcr.Row, ycbcr.Column);
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

        private static bool IsFinishCharByte(int charBinaryIndex) => charBinaryIndex == 8;
    }
}
