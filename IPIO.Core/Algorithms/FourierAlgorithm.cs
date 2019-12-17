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
        private readonly double ALPHA = 0.1;
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
                    EmbeddFormula(coefficient.Value, watermarkValue);
            });

            return originalImage;
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
                var originalArray = originalImage.ToComplexArray();
                var watermarkArray = watermarkedImage.ToComplexArray();

                var dftOriginal = originalArray.MapForBlock(_dftTransform.Execute);
                var dftWaterked = watermarkArray.MapForBlock(_dftTransform.Execute);

                var watermark = Extract(dftWaterked, dftOriginal, 64);
                return watermark.ToBitmap();
            });
        }

        private Complex[,] Extract(Complex[,] dftWatermarked, Complex[,] originalImage, int size)
        {
            var bestCoeficients = BestCoeficients(originalImage);

            Complex[,] watermark = new Complex[size, size];

            for (int i = 0; i < size * size; i++)
            {
                var coefficient = bestCoeficients[i];
                var originalValue = originalImage[coefficient.I, coefficient.J];
                var encodedValue = dftWatermarked[coefficient.I, coefficient.J];
                watermark[i / size, i % size] = ExtractFormula(encodedValue, originalValue);
            }

            return watermark;

        }

        private Complex EmbeddFormula(Complex originalvalue, Complex watermarkValue)
        {
            return originalvalue + ALPHA * Complex.Abs(originalvalue) * watermarkValue;
        }

        public Complex ExtractFormula(Complex encodedValue, Complex originalValue)
        {
            var value = ALPHA * Complex.Abs(originalValue);
            value = value == 0 ? double.MaxValue : value;
            return (encodedValue - originalValue) / value;
        }
    }
}
