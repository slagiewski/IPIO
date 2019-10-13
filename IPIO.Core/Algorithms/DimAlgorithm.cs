using IPIO.Core.Extensions;
using IPIO.Core.Models;
using System.Drawing;

namespace IPIO.Core.Algorithms
{
    public class DimAlgorithm : IWatermarkingAlgorithm
    {
        public Bitmap Run(Bitmap bitmap)
        {
            return bitmap.Select(pixel =>
                new Pixel(
                    pixel.R / 2,
                    pixel.G / 2,
                    pixel.B / 2,
                    pixel.Alpha.HasValue ? pixel.Alpha : default(int?)
                    )
                );
        }
    }
}
