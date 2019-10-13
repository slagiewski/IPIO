using System.Drawing;

namespace IPIO.Core.Algorithms
{
    public interface IWatermarkingAlgorithm
    {
        Bitmap Run(Bitmap bitmap);
    }
}
