using System.Numerics;

namespace IPIO.Core.Algorithms.Formulas
{
    public class BasicMultiplicationFormula : IFormula
    {
        public BasicMultiplicationFormula(double alpha) => Alpha = alpha;

        public static string Name => "o + α * w";

        public double Alpha { get; }

        public double GetTransformedValue(double originalValue, double watermarkValue) => originalValue + Alpha * watermarkValue;

        public double RetrieveTransformedValue(double encodedValue, double originalValue) => (encodedValue - originalValue) / Alpha;

        public Complex GetTransformedValue(Complex originalValue, Complex watermarkValue) => originalValue + Alpha * watermarkValue;

        public Complex RetrieveTransformedValue(Complex encodedValue, Complex originalValue) => (encodedValue - originalValue) / Alpha;
    }
}