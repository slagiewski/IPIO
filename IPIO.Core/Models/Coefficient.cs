namespace IPIO.Core.Models
{
    public class Coefficient<Complex>
    {
        public Complex Value { get; }
        public int I { get; }
        public int J { get; }

        public Coefficient(Complex value, int i, int j)
        {
            Value = value;
            I = i;
            J = j;
        }
    }
}