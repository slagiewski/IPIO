using System;
using System.Numerics;

namespace IPIO.Core.Algorithms.Formulas
{
    public class AbsExtendedBasicMultiplicationFormula : IFormula
    {
        public AbsExtendedBasicMultiplicationFormula(double alpha) => Alpha = alpha;

        public static string Name => "o + α * |o| * w";

        public double Alpha { get; }

        public double GetTransformedValue(double originalValue, double watermarkValue) =>
            originalValue + Alpha * Math.Abs(originalValue) * watermarkValue;

        public double RetrieveTransformedValue(double encodedValue, double originalValue) =>
            (encodedValue - originalValue) /
            (Alpha * DivisionSecureAbs(originalValue));

        public Complex GetTransformedValue(Complex originalValue, Complex watermarkValue) =>
            originalValue + Alpha * Complex.Abs(originalValue) * watermarkValue;

        public Complex RetrieveTransformedValue(Complex encodedValue, Complex originalValue) =>
            (encodedValue - originalValue) /
            (Alpha * DivisionSecureAbs(originalValue));

        private static double DivisionSecureAbs(Complex originalValue) =>
            Complex.Abs(originalValue) == 0 ? double.MaxValue : Complex.Abs(originalValue);

        private static double DivisionSecureAbs(double originalValue) =>
            originalValue == 0 ? double.MaxValue : Math.Abs(originalValue);
    }
}