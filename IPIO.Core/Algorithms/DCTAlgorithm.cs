using IPIO.Core.Algorithms.Formulas;
using IPIO.Core.Extensions;
using IPIO.Core.Interfaces;
using IPIO.Core.Models;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;

namespace IPIO.Core.Algorithms
{
    public class DctAlgorithm : IWatermarkingAlgorithm
    {
        public IFormula Formula { get; }

        public DctAlgorithm(IFormula encodingFormula)
        {
            Formula = encodingFormula;
        }

        public async Task<Bitmap> EmbedAsync(Bitmap originalImage, Bitmap message)
        {
            return await Task.Run(() =>
            {
                var originalBitmapData = LockBitmap(originalImage);
                var messageBitmapData = LockBitmap(message);

                var newPixels = new Pixel[originalImage.Width * originalImage.Height];

                var messageIndex = -1;
                var messagePixels = messageBitmapData.ToPixels();

                var originalImageBlocks = originalBitmapData.ToBlocks();

                var blocks = originalImageBlocks.GetBlocksCoefficientsWithHighestDCTValues(messagePixels.Length);

                blocks.ForEach(b =>
                {
                    messageIndex++;
                    b.EmbedByte(messagePixels[messageIndex].B, Formula);
                });

                originalImageBlocks.Content.ForEach(block =>
                {
                    BlockExtensions.CopyBlockIntoPixelsArray(block, originalImage.Width, newPixels);
                });

                var modifiedBitmapBytes = new byte[GetBytesCount(originalBitmapData)];

                newPixels.CopyToByteArray(modifiedBitmapBytes, originalBitmapData.Stride);

                originalImage.UnlockBits(originalBitmapData);
                message.UnlockBits(messageBitmapData);

                var bitmap = modifiedBitmapBytes.ToBitmap(originalBitmapData.Width, originalBitmapData.Height, originalImage.PixelFormat);

                return bitmap;
            });
        }

        public async Task<Bitmap> RetrieveAsync(Bitmap originalImage, Bitmap watermarkedImage, int watermarkLength)
        {
            return await Task.Run(() =>
            {
                var watermarkedBitmapData = LockBitmap(watermarkedImage);
                var originalBitmapData = LockBitmap(originalImage);

                var watermarkedBlocks = watermarkedBitmapData.ToBlocks();
                var originalBlocks = originalBitmapData.ToBlocks();

                var blocks = originalBlocks.GetBlocksCoefficientsWithHighestDCTValues(watermarkLength);

                var newPixels = new Pixel[watermarkLength];

                var messageIndex = 0;
                blocks.ForEach(b =>
                {
                    if (messageIndex == watermarkLength)
                    {
                        return;
                    }

                    var transformedBlock = watermarkedBlocks.Content.First(wb => b.Column == wb.Column && b.Row == wb.Row);

                    var result = transformedBlock.GetEmbeddedByte(b, Formula);

                    newPixels[messageIndex] = new Pixel(result, result, result, messageIndex / 64, messageIndex % 64);

                    messageIndex++;
                });

                originalImage.UnlockBits(originalBitmapData);
                watermarkedImage.UnlockBits(watermarkedBitmapData);

                var modifiedBitmapBytes = new byte[watermarkLength * 3];

                newPixels.CopyToByteArray(modifiedBitmapBytes, 64 * 3);

                var bitmap = modifiedBitmapBytes.ToBitmap(64, 64, originalImage.PixelFormat);

                return bitmap;
            });
        }

        private static BitmapData LockBitmap(Bitmap originalImage) =>
            originalImage.LockBits(
                new Rectangle(0, 0, originalImage.Width, originalImage.Height),
                ImageLockMode.ReadWrite,
                originalImage.PixelFormat);

        private static int GetBytesCount(BitmapData bitmapData)
        {
            var distanceBetweenVerticalPixels = bitmapData.Stride;
            return distanceBetweenVerticalPixels * bitmapData.Height;
        }
    }
}