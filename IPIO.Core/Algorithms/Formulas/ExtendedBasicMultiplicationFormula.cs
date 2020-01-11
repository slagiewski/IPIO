using System.Numerics;

namespace IPIO.Core.Algorithms.Formulas
{
    public class ExtendedBasicMultiplicationFormula : IFormula
    {
        public ExtendedBasicMultiplicationFormula(double alpha) => Alpha = alpha;

        public static string Name => "o + α * o * w";

        public double Alpha { get; }

        public double GetTransformedValue(double originalValue, double watermarkValue) =>
            originalValue + Alpha * originalValue * watermarkValue;

        public double RetrieveTransformedValue(double encodedValue, double originalValue) =>
            (encodedValue - originalValue) /
            (Alpha * DivisionSecureValue(originalValue));

        public Complex GetTransformedValue(Complex originalValue, Complex watermarkValue) =>
            originalValue + Alpha * originalValue * watermarkValue;

        public Complex RetrieveTransformedValue(Complex encodedValue, Complex originalValue) =>
            (encodedValue - originalValue) /
            (Alpha * DivisionSecureValue(originalValue));

        private Complex DivisionSecureValue(Complex originalValue) =>
            originalValue == 0 ? double.MaxValue : originalValue;

        private static double DivisionSecureValue(double originalValue) =>
            originalValue == 0 ? double.MaxValue : originalValue;
    }
}