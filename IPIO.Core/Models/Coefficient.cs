using System;
using System.Collections.Generic;
using System.Text;

namespace IPIO.Core.Models
{
    public class Coefficient<T>
    {
        public T Value { get; }
        public int I { get; }
        public int J { get; }

        public Coefficient(T value, int i, int j)
        {
            Value = value;
            I = i;
            J = j;
        }
    }
}
