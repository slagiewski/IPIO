using System;
using System.Collections.Generic;
using System.Text;

namespace IPIO.Core.Models
{
    public struct YCbCr
    {
        public float Y { get; set; }
        public readonly float Cb { get; }
        public readonly float Cr { get; }
        public readonly int Row { get; }
        public readonly int Column { get; }

        public YCbCr(float y, float cb, float cr)
        {
            Y = y;
            Cb = cb;
            Cr = cr;
            Row = 0;
            Column = 0;
        }
        public YCbCr(float y, float cb, float cr, int row, int column)
        {
            Y = y;
            Cb = cb;
            Cr = cr;
            Row = row;
            Column = column;
        }


    }
}
