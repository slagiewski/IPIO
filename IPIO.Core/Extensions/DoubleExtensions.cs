using System;

namespace IPIO.Core.Extensions
{
    public static class DoubleExtensions
    {
        public static int ToInt(this double value)
        {
            try
            {
                var int32 = Convert.ToInt32(value);
                return int32;
            }
            catch
            {
                return value > 0 ? Int32.MaxValue : Int32.MinValue;
            }
        }
    }
}