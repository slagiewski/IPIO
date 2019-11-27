using System.Collections.Generic;

namespace IPIO.Core.Models
{
    public class Blocks
    {
        public Blocks(List<Block> blocks, int bitmapWidth, int bitmapHeight, int blockWidth, int blockHeight)
        {
            Content = blocks;
            OriginalWidthInPixels = bitmapWidth;
            OriginalHeightInPixels = bitmapHeight;
            BlockWidth = blockWidth;
            BlockHeight = blockHeight;
        }

        public List<Block> Content { get; set; }

        public int OriginalWidthInPixels { get; set; }

        public int OriginalHeightInPixels { get; set; }

        public int BlockWidth { get; set; }

        public int BlockHeight { get; set; }

        public int NumberOfColumns => OriginalWidthInPixels / BlockWidth;

        public int NumberOfRows => OriginalHeightInPixels / BlockHeight;

        public int NumberOfPixels => NumberOfColumns * NumberOfRows * BlockHeight * BlockWidth;

    }
}
