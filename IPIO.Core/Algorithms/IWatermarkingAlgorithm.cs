using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace IPIO.Core.Algorithms
{
    public interface IWatermarkingAlgorithm
    {
        Bitmap Run(Bitmap bitmap);
    }
}
