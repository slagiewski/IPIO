using IPIO.Core.Models;
using IPIO.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace IPIO.Extensions
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
            var ap = DCTTools.CalculateAP(block.Row, block.BlockWidth);
            var aq = DCTTools.CalculateAQ(block.Column, block.BlockHeight);

            var sum = 0.0;
            List<Pixel> pixels = (List<Pixel>)block.Pixels;

            // Od lewej do prawej i potem w dół każdą następną wartością sumy zastępuję Pixel.B
            for (var j = 0; j < block.BlockWidth; j++)
            {
                for (var i = 0; i < block.BlockHeight; i++)
                {
                    sum += Math.Cos((Math.PI * (2 * i + 1) * block.Column) / (2 * block.BlockWidth)) *
                        Math.Cos((Math.PI * (2 * j + 1) * block.Row) / (2 * block.BlockHeight));

                    var pixel = pixels[i + j * block.BlockWidth];
                    pixels[i + j * block.BlockWidth] = new Pixel(pixel.R, pixel.G, (int)(sum * ap * aq), pixel.Alpha, pixel.Row, pixel.Column);
                }
            }

            // W ostatnim pixelu koduję pod Pixel.B zakodowaną wartość parametru
            var coded = DCTTools.GetByteWithModifiedLsb((int)(sum * ap * aq), newLsb);
            var lastPixel = pixels[pixels.Count - 1];
            pixels[pixels.Count - 1] = new Pixel(lastPixel.R, lastPixel.G, coded, lastPixel.Alpha, lastPixel.Row, lastPixel.Column);


            // Po zakodowaniu według wzrou wracam wszystko do normalnych wartości
            sum = 0.0;
            for (var j = 0; j < block.BlockWidth; j++)
            {
                for (var i = 0; i < block.BlockHeight; i++)
                {
                    var pixel = pixels[i + j * block.BlockWidth];
                    sum += ap * aq * pixel.B * Math.Cos((Math.PI * (2 * i + 1) * block.Column) / (2 * block.BlockWidth)) *
                        Math.Cos((Math.PI * (2 * j + 1) * block.Row) / (2 * block.BlockHeight));

                    pixels[i + j * block.BlockWidth] = new Pixel(pixel.R, pixel.G, (int)sum, pixel.Alpha, pixel.Row, pixel.Column);
                }
            }

            return new Block(pixels, block.BlockWidth, block.BlockHeight, block.Column, block.Row);
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
            var ap = DCTTools.CalculateAP(block.Row, block.BlockWidth);
            var aq = DCTTools.CalculateAQ(block.Column, block.BlockHeight);

            var sum = 0.0;
            List<Pixel> pixels = (List<Pixel>)block.Pixels;
            for (var j = 0; j < block.BlockWidth; j++)
            {
                for (var i = 0; i < block.BlockHeight; i++)
                {
                    sum += Math.Cos((Math.PI * (2 * i + 1) * block.Column) / (2 * block.BlockWidth)) *
                        Math.Cos((Math.PI * (2 * j + 1) * block.Row) / (2 * block.BlockHeight));
                }
            }

            sum *= sum * ap * aq;
            return DCTTools.GetLsb((int)sum);
        }
    }
}

