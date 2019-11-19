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
        int BLOCK_SIZE = 8;
        public async Task<Bitmap> EmbedAsync(Bitmap bitmap, string message)
        {
            var messageWithEndChar = message + '\0';
            var charBinaryIndex = 0;
            var charIndex = 0;
            int charValue = messageWithEndChar[charIndex];

            bool MessageAlreadyEmbedded() => charIndex == messageWithEndChar.Length;

            return await Task.Run(() =>
            {
                // 1. Podziel obrazek na 8x8
                var blocks = DivideImageIntoBlocks(bitmap, BLOCK_SIZE);

                // 2. W kazdym bloku wybierz najmniej znaczacy bit
                var dctBlocks = new List<DCTBlock>();
                blocks.ForEach(block =>
                {
                    dctBlocks.Add(DCTTransformBlock(block));
                });
                blocks.Clear();

                // 3. Wez najmniej znaczacy bit z litery z aktualnego slowa
                dctBlocks.ForEach(dctBlock =>
                {
                    if (!MessageAlreadyEmbedded())
                    {
                        ReplaceLSBToDC(dctBlock, GetLsb(charValue));
                        charValue = ExposeNextBit(charValue);

                        if (IsEndOfChar(charBinaryIndex))
                        {
                            charIndex++;
                            charBinaryIndex = 0;
                            charValue = messageWithEndChar[charIndex];
                        }
                        else
                        {
                            charBinaryIndex++;
                        }
                    }
                });
                // 4. Teraz mamy zakodowane bity slowa w najmniej znaczacych bitach blokow
                // Odkodowujemy bloki
                dctBlocks.ForEach(dctBlock =>
                {
                    blocks.Add(TransformBlockBackToPixels(dctBlock));
                });
                // 5. Bloki pixeli składamy z powrotem w obrazek
                var embeddedBitmap = ConstructBitmapFromBlocks(blocks);
                return embeddedBitmap;
            });
        }

        private List<Pixel[][]> DivideImageIntoBlocks(Bitmap bitmap, int size)
        {

        }

        private DCTBlock DCTTransformBlock(Pixel[][] pixelBlock)
        {

        }

        private Pixel[][] TransformBlockBackToPixels(DCTBlock dCTBlock)
        {

        }

        private void ReplaceLSBToDC(DCTBlock dCTBlock, int charValue)
        {

        }

        private Bitmap ConstructBitmapFromBlocks(List<Pixel[][]> pixelBlocks)
        {

        }

        public Task<string> RetrieveAsync(Bitmap bitmap)
        {
            throw new NotImplementedException();
        }

        private static int GetLsb(int value) => value % 2;
        private static int ExposeNextBit(int value) => value / 2;
        private static bool IsEndOfChar(int charBinaryIndex) => charBinaryIndex == 8;
    }
     
}
