using System;
using System.Numerics;

namespace IPIO.Core.Extensions
{
    public static class ComplexExtensions
    {
        private static readonly int BLOCK_SIZE = 8;

        public static Complex[,] MapForBlock(this Complex[,] array, Func<Complex[,], Complex[,]> transformation)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);

            var result = new Complex[width, height];
            array.ActionForBlock((slice, startW, startH) =>
            {
                var transform = transformation(slice);
                transform.CopyTo(result, 0, BLOCK_SIZE - 1, 0, BLOCK_SIZE - 1, startW, startH);
            });

            return result;
        }

        public static Complex[,] Slice<Complex>(this Complex[,] values, int rowMin, int rowMax, int colMin, int colMax)
        {
            int numRows = rowMax - rowMin + 1;
            int numCols = colMax - colMin + 1;
            Complex[,] result = new Complex[numRows, numCols];

            int totalCols = values.GetUpperBound(1) + 1;
            int fromIndex = rowMin * totalCols + colMin;
            int toIndex = 0;
            for (int row = 0; row <= numRows - 1; row++)
            {
                Array.Copy(values, fromIndex, result, toIndex, numCols);
                fromIndex += totalCols;
                toIndex += numCols;
            }

            return result;
        }

        public static void ActionForBlock<Complex>(this Complex[,] array, Action<Complex[,], int, int> transformation, int blockSize = 8)
        {
            var width = array.GetLength(0);
            var height = array.GetLength(1);

            var blockLengthIndex = blockSize - 1;
            for (int w = 0; w < width / blockSize; w++)
            {
                for (int h = 0; h < height / blockSize; h++)
                {
                    var startW = w * blockSize;
                    var startH = h * blockSize;
                    var slice = array.Slice(startW, startW + blockLengthIndex, startH, startH + blockLengthIndex);
                    transformation(slice, startW, startH);
                }
            }
        }

        public static void CopyTo<Complex>(this Complex[,] fromArray, Complex[,] toArray,
            int fromRowMin, int fromRowMax, int fromColMin,
            int fromColMax, int toRowMin, int toColMin)
        {
            int fromNumCols = fromArray.GetUpperBound(1) + 1;
            int toNumCols = toArray.GetUpperBound(1) + 1;
            int numRows = fromRowMax - fromRowMin + 1;
            int numCols = fromColMax - fromColMin + 1;
            int fromIndex = fromRowMin * fromNumCols + fromColMin;
            int toIndex = toRowMin * toNumCols + toColMin;

            for (int row = 0; row <= numRows - 1; row++)
            {
                Array.Copy(fromArray, fromIndex, toArray, toIndex, numCols);
                fromIndex += fromNumCols;
                toIndex += toNumCols;
            }
        }

        public static void ForEach<Complex>(this Complex[,] data, Action<Complex, Tuple<int, int>> indexedLambda)
        {
            var width = data.GetLength(0);
            var height = data.GetLength(1);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    indexedLambda(data[i, j], new Tuple<int, int>(i, j));
                }
            }
        }
    }
}