using System.Collections.Generic;

namespace IPIO.Core.Models
{
    public class Block
    {
        public Block(IEnumerable<Pixel> pixels, int blockWidth, int blockHeight, int column, int row)
        {
            Pixels = pixels;
            BlockWidth = blockWidth;
            BlockHeight = blockHeight;
            Column = column;
            Row = row;
        }

        public IEnumerable<Pixel> Pixels { get; }
        public int BlockWidth { get; }
        public int BlockHeight { get; }
        public int Column { get; set; }
        public int Row { get; set; }
    }
}