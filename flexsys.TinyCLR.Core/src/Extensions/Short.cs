using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace flexsys.TinyCLR.Core.Extensions
{
    public static class Short
    {
        public static byte[] GetBytes(this short value)
        {
            return new byte[2] { (byte)(value & 0xFF), (byte)((value >> 8) & 0xFF) };
        }
    }
}
