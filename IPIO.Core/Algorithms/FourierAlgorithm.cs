﻿using IPIO.Core.Extensions;
using IPIO.Core.Interfaces;
using IPIO.Core.Models;
using IPIO.Core.Transform;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace IPIO.Core.Algorithms
{
    public class FourierAlgorithm : IWatermarkingAlgorithm
    {
        private DftTransform _dftTransform = new DftTransform();
        public async Task<Bitmap> EmbedAsync(Bitmap originalImage, Bitmap watermark)
        {
            return await Task.Run(() =>
            {
                var originalArray = originalImage.ToComplexArray();
                var watermarkArray = watermark.ToComplexArray();

                var dft = Transform(originalArray);
                var embedded = Embedd(dft, watermarkArray);
                var idft = ReverseTransform(embedded);

                return idft.ToBitmap();
            });
        }

        private Complex[,] Embedd(Complex[,] originalImage, Complex[,] watermark)
        {
            var width = watermark.GetLength(0);
            var bestCoeficients = BestCoeficients(originalImage);

            watermark.ForEach((watermarkValue, index) =>
            {
                var vectorIndex = index.Item1 * width + index.Item2;
                var coefficient = bestCoeficients[vectorIndex];
                originalImage[coefficient.I, coefficient.J] =
                    Formula(coefficient.Value, watermarkValue);


                //originalImage[coefficient.I + 20, coefficient.J + 20] =
                //    Formula(originalImage[coefficient.I + 20, coefficient.J + 20], watermarkValue);

            });

            return originalImage;
        }

        private Complex Formula(Complex originalvalue, Complex watermarkValue)
        {
            return originalvalue + 0.1 * Complex.Abs(originalvalue) * watermarkValue;
        }

        private static List<Coefficient<Complex>> BestCoeficients(Complex[,] originalImage)
        {
            var bestCoeficients = new List<Coefficient<Complex>>();

            originalImage.DoForBlock((slice, startW, startH) =>
            {
                bestCoeficients.Add(new Coefficient<Complex>(slice[1, 1], startW + 1, startH + 1));
            });

            bestCoeficients.Sort((x, y) => Complex.Abs(y.Value).CompareTo(Complex.Abs(x.Value)));
            return bestCoeficients;
        }

        private Complex[,] Transform(Complex[,] array)
        {
            return array.MapForBlock(_dftTransform.Execute);
        }
        private Complex[,] ReverseTransform(Complex[,] array)
        {
            return array.MapForBlock(_dftTransform.ExecuteInverse);
        }



        public async Task<Bitmap> RetrieveAsync(Bitmap originalImage, Bitmap watermarkedImage, int watermarkLength)
        {
            return await Task.Run(() =>
            {


                return watermarkedImage; //CHANGE
            });
        }
    }
}