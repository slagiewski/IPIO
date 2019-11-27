using System;
using System.Collections.Generic;
using System.Text;

namespace IPIO.Utils
{
    public class DCTTools
    {
        public static double CalculateAP(int blockRow, int blockWidth)
        {
            return blockRow == 0 ? 1 / Math.Sqrt(blockWidth) : Math.Sqrt(2 / blockWidth);
        }

        public static double CalculateAQ(int blockColumn, int blockHeight)
        {
            return blockColumn == 0 ? 1 / Math.Sqrt(blockHeight) : Math.Sqrt(2 / blockHeight);
        }

        public static int GetByteWithModifiedLsb(int value, int newLsb)
        {
            if (newLsb == 1)
            {
                return value | newLsb;
            }
            else
            {
                return value & ~1;
            }
        }

        public static int GetLsb(int value) => value % 2;
    }
}
