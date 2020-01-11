using System;
using System.Collections.Generic;
using System.Text;

namespace IPIO.Core.Models
{
    public class AlgorithmEfficiency
    {
        public AlgorithmEfficiency(double psnr, double mse)
        {
            PSNR = psnr;
            MSE = mse;
        }

        public double PSNR { get; }

        public double MSE { get; }
    }
}
