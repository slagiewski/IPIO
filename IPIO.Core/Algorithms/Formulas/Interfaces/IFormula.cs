using System.Numerics;

namespace IPIO.Core.Algorithms.Formulas
{
    public interface IFormula
    {
        public double Alpha { get; }

        public static string Name { get; }

        double GetTransformedValue(double originalValue, double watermarkValue);
        double RetrieveTransformedValue(double encodedValue, double originalValue);

        Complex GetTransformedValue(Complex originalValue, Complex watermarkValue);
        Complex RetrieveTransformedValue(Complex encodedValue, Complex originalValue);
    }
}
