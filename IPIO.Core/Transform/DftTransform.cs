using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace IPIO.Core.Transform
{
    public class DftTransform
    {
        public Complex[,] Transform(Complex[,] data)
        {
            var width = data.GetLength(0);
            var height = data.GetLength(1);

            Complex[,] result = new Complex[width, height];

            for (int u = 0; u < width; u++)
            {
                for (int v = 0; v < height; v++)
                {
                    var sum = CalcSum(data, width, height, u, v, Direction.Normal);
                    result[u, v] = sum;
                }
            }

            return result;
        }

        private Complex CalcSum(Complex[,] data, int width, int height, int u, int v,
            Direction direction)
        {
            var sum = Complex.Zero;
            var coefU = Multi(direction) * 2 * Math.PI * u / width;
            var coefV = Multi(direction) * 2 * Math.PI * v / height;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var first = Complex.Exp(new Complex(0, i * coefU));
                    var second = Complex.Exp(new Complex(0, j * coefV));
                    sum += first * second * data[i, j];
                }
            }

            return sum;
        }

        public Complex[,] UndoTransform(Complex[,] data)
        {
            var width = data.GetLength(0);
            var height = data.GetLength(1);

            Complex[,] result = new Complex[width, height];

            for (int u = 0; u < width; u++)
            {
                for (int v = 0; v < height; v++)
                {
                    var sum = CalcSum(data, width, height, u, v, Direction.Inverse);
                    result[u, v] = sum / (width * height);
                }
            }

            return result;
        }

        private int Multi(Direction direction)
        {
            switch (direction)
            {
                case Direction.Normal:
                    return -1;
                case Direction.Inverse:
                    return 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        enum Direction
        {
            Normal,
            Inverse
        }
    }
}
