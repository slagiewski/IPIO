using System;

namespace IPIO.Core.Utils
{
    public class DCTTools
    {
        public static double CalculateAP(int pIndex, int blockWidth)
        {
            return pIndex == 0 ? 1 / Math.Sqrt(blockWidth) : Math.Sqrt(2 / (double)blockWidth);
        }

        public static double CalculateAQ(int qIndex, int blockHeight)
        {
            return qIndex == 0 ? 1 / Math.Sqrt(blockHeight) : Math.Sqrt(2 / (double)blockHeight);
        }
    }
}
