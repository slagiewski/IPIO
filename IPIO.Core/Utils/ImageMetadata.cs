using IPIO.Core.Extensions;
using IPIO.Core.Models;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace IPIO.Core.Utils
{
    public class ImageMetadata
    {
        public async static Task<AlgorithmEfficiency> GetAlgorithmEfficiencyAsync(Bitmap originalImage, Bitmap imageWithMessage)
        {
            return await Task.Run(() =>
            {
                var originalImageBitmapData = LockBitmap(originalImage);
                var imageWithMessageBitmapData = LockBitmap(imageWithMessage);

                var originalPixels = originalImageBitmapData.ToPixels();
                var imageWithMessagePixels = imageWithMessageBitmapData.ToPixels();

                var mseSum = 0d;

                for (var i = 0; i < originalPixels.Length; i++)
                {
                    mseSum += Math.Pow(originalPixels[i].B - imageWithMessagePixels[i].B, 2);
                }

                var mse = mseSum / (originalImage.Width * originalImage.Height);

                originalImage.UnlockBits(originalImageBitmapData);
                imageWithMessage.UnlockBits(imageWithMessageBitmapData);

                return new AlgorithmEfficiency(
                        10 * Math.Log10(Math.Pow(255, 2) / mse),
                        mse
                    );
            });

        }

        private static BitmapData LockBitmap(Bitmap image) =>
            image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadWrite,
                image.PixelFormat);
    }
}
