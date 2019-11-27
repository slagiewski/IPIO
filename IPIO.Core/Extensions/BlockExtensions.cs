using IPIO.Core.Models;
using IPIO.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPIO.Core.Extensions
{
    public static class BlockExtensions
    {
        /// <summary>
        /// Wzory brane z artykułu : https://www.researchgate.net/publication/317401791_Secure_Image_Steganography_Algorithm_Based_on_DCT_with_OTP_Encryption?fbclid=IwAR2MBg3TnKRYtkQ3w9gkbx0gjBlViiAn6sfjXe9VJ2p5etlYzsh3Ca_p3h4
        /// Miały najbardziej do mnie przekonywujące te wzory.
        /// 
        /// Ogólnie na początku wyliczane są te współczynniki ap i aq
        /// Potem pixel po pixelu lecimy od lewej do prawej i potem coraz bardziej w dół
        /// W każdym następnym pixelu sumą zastępuję wartość [Pixel.B]
        /// Na sam koniec podmieniam ostatni bit współczynnika z bitem znaku
        /// Podmieniam ostatni pixel na ten z zakodowanym [Pixel.B] 
        ///
        /// Potem algorytmem inverse jadę od nowa i koduję zgodnie ze wzorem wartośc z powrotem
        /// Na koniec zwracam [Block] z podmienioną listą pixeli na te po wszystkich wzorach
        /// </summary>
        /// <param name="block">Świeży block z pixelami</param>
        /// <param name="newLsb">Wartość znaku, którą trzeba zakodować w tym bloku</param>
        /// <returns></returns>
        public static Block EmbedChar(this Block block, int newLsb)
        {
            var blueDct = block.ToDct();

            blueDct[^1] = newLsb;

            var newBlock = blueDct.FromDct(block.BlockWidth, block.BlockHeight);

            for (int i = 0; i < block.Pixels.Count; i++)
            {
                var oldPixel = block.Pixels[i];
                block.Pixels[i] = new Pixel(oldPixel.R, oldPixel.G, (byte)newBlock[i], oldPixel.Row, oldPixel.Column);
            }

            return block;
        }

        private static double[] ToDct(this Block block)
        {
            var dct = new double[block.BlockWidth * block.BlockHeight];

            for (int p = 0; p < block.BlockWidth; p++)
            {
                for (int q = 0; q < block.BlockHeight; q++)
                {
                    var ap = DCTTools.CalculateAP(p, block.BlockWidth);
                    var aq = DCTTools.CalculateAQ(q, block.BlockHeight);

                    var sum = 0.0;
                    for (var k = 0; k < block.BlockWidth; k++)
                    {
                        for (var l = 0; l < block.BlockHeight; l++)
                        {
                            sum += block.Pixels[l + k * block.BlockWidth].B *
                                   Math.Cos((Math.PI * (2 * l + 1) * q) / (2 * block.BlockWidth)) *
                                   Math.Cos((Math.PI * (2 * k + 1) * p) / (2 * block.BlockHeight));

                        }
                    }

                    dct[p * block.BlockWidth + q] = ap * aq * sum;
                }
            }

            return dct;
        }

        private static int[] FromDct(this double[] dctBLock, int blockWidth, int blockHeight)
        {
            var blueOfPixels = new int[dctBLock.Length];

            for (int p = 0; p < blockWidth; p++)
            {
                for (int q = 0; q < blockHeight; q++)
                {
                    var ap = DCTTools.CalculateAP(p, blockWidth);
                    var aq = DCTTools.CalculateAQ(q, blockHeight);

                    var blueOfPixel = 0.0;
                    for (var k = 0; k < blockWidth; k++)
                    {
                        for (var l = 0; l < blockHeight; l++)
                        {
                            blueOfPixel += ap * aq * dctBLock[p * blockWidth + q] * 
                                   Math.Cos((Math.PI * (2 * l + 1) * q) / (2 * blockWidth)) *
                                   Math.Cos((Math.PI * (2 * k + 1) * p) / (2 * blockHeight));

                        }
                    }

                    blueOfPixels[p * blockWidth + q] = (int)blueOfPixel;
                }
            }

            return blueOfPixels;
        }

        /// <summary>
        /// Zwraca ostatni bit z wartości Blue Pixela.
        /// Robiłem to na modłe taką, żeby z każdego bloku można było wyciągnąć jak w LSB
        /// Np. w ten sposób :
        /// 
        /// [LsbAlgorithm.cs]
        /// 105. var charBit = GetLsb(pixel.R);
        /// 106. charValue += charBit* (int) Math.Pow(2, charBinaryIndex++);
        /// 
        /// 
        /// </summary>
        /// <param name="block">Block z zakodowaną informacją</param>
        /// <returns>Bit zakodowanego znaku ASCII</returns>
        public static int GetCharBit(this Block block)
        {
            var blueDct = block.ToDct();

            var newLsb = blueDct[^1];

            return (int)Math.Round(newLsb);
        }
    }
}

