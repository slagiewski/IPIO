using Accord.Math.Wavelets;
using IPIO.Core.Extensions;
using IPIO.Core.Interfaces;
using IPIO.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Threading.Tasks;
using Haar = Accord.Math.Wavelets.Haar;

namespace IPIO.Core.Algorithms
{
    public class HaarWaveletAlgorithm : IWatermarkingAlgorithm
    {
        readonly int MAX_DEPTH = 4;
      
        public async Task<Bitmap> EmbedAsync(Bitmap originalImage, Bitmap message)
        {
            return await Task.Run(() =>
            {
                var originalBitmapData = LockBitmap(originalImage);
                var messageBitmapData = LockBitmap(message);

                var newPixels = new Pixel[originalImage.Width * originalImage.Height];

                var messageIndex = -1;
                var messagePixels = messageBitmapData.ToPixels();

                var blueOfPixels = new List<double>();

                //ograniczyć się do rozmiaru wiadomości.. 
                var pixels = originalBitmapData.ToPixels();
                foreach (var pixel in pixels)
                {
                    blueOfPixels.Add(pixel.B);
                }
                var arrayBlueOfPixels = blueOfPixels.ToArray();

                var originalHaar = new Haar(MAX_DEPTH);
                originalHaar.Forward(arrayBlueOfPixels);

                for (var index = 0; index < messagePixels.Length; index++)
                {

                    arrayBlueOfPixels[index] = GetTransformedValue(arrayBlueOfPixels[index], messagePixels[index].B);
                }


                originalHaar.Backward(arrayBlueOfPixels);
                originalImage.UnlockBits(originalBitmapData);
                message.UnlockBits(messageBitmapData);
                var bitmap = originalImage;
                var x = 0;
                bitmap.Select(p =>
                {
                    if (x < messagePixels.Length)
                    {
                        return new Pixel(p.R, p.G, Math.Max((byte)0, Math.Min((byte)255, (byte)arrayBlueOfPixels[x++])), p.Row, p.Column);
                    }
                    return p;
                });


                return bitmap;
            });
        }

        public async Task<Bitmap> RetrieveAsync(Bitmap originalImage, Bitmap watermarkedImage, int watermarkLength)
        {
            return await Task.Run(() =>
            {

                var originalBitmapData = LockBitmap(originalImage);
                var originalPixels = originalBitmapData.ToPixels();
                var newPixels = new Pixel[watermarkLength];
                var blueOfOriginals = new List<double>();
                var haar = new Haar(MAX_DEPTH);

                for (var i = 0; i < watermarkLength; i++)
                {
                    blueOfOriginals.Add(originalPixels[i].B);
                }

                var arrayOfBlues = blueOfOriginals.ToArray();
                haar.Forward(arrayOfBlues);
                for (var j = 0; j < arrayOfBlues.Length; j++)
                {
                    newPixels[j] = new Pixel(0, 0, (byte)arrayOfBlues[j], j / 64, j % 64);
                }

                originalImage.UnlockBits(originalBitmapData);
                var modifiedBitmapBytes = new byte[watermarkLength * 3];

                newPixels.CopyToByteArray(modifiedBitmapBytes, 64 * 3);

                var bitmap = modifiedBitmapBytes.ToBitmap(64, 64, originalImage.PixelFormat);

                return bitmap;
            });
        }


        private double EmbedByte(double originalValue, double watermarkValue)
        {
            return GetTransformedValue(originalValue, watermarkValue);
        }

        private static BitmapData LockBitmap(Bitmap originalImage) =>
          originalImage.LockBits(
              new Rectangle(0, 0, originalImage.Width, originalImage.Height),
              ImageLockMode.ReadWrite,
              originalImage.PixelFormat);


        private static double GetTransformedValue(double originalValue, double watermarkValue) =>
           originalValue + 0.1 * watermarkValue;

        private static double RetrieveTransformedValue(double encodedValue, double originalValue)
        {
            return (encodedValue - originalValue) / 0.1;
        }
    }


}
