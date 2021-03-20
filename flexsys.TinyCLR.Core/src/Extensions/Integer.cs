using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace flexsys.TinyCLR.Core.Extensions
{
    public static class Integer
    {
        public static byte[] GetBytes(this int value)
        {
            return new byte[4] { (byte)(value & 0xFF), (byte)((value >> 8) & 0xFF), (byte)((value >> 16) & 0xFF), (byte)((value >> 24) & 0xFF) };
        }
    }
}
