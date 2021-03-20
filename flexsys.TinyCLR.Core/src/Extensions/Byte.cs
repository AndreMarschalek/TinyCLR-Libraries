using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace flexsys.TinyCLR.Core.Extensions
{
    public static class Byte
    {
        public static bool Equal(this byte[] Array1, int Start1, byte[] Array2, int Start2, int Count)
        {
            bool result = true;
            for (int i = 0; i < Count - 1; i++)
            {
                if (Array1[i + Start1] != Array2[i + Start2])
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        public static long RealByteCount(this byte[] bytes)
        {
            try
            {
                return (long)Encoding.UTF8.GetChars(bytes).Length;
            }
            catch
            {
                return 0L;
            }
        }

        public static byte[] Reverse(this byte[] source)
        {
            byte[] reversed = new byte[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                reversed[i] = source[source.Length - 1 - i];
            }

            return reversed;
        }

        public static byte[] ReverseByteOrder(this byte[] bytes)
        {
            byte temp;
            int highCtr = bytes.Length - 1;

            for (int ctr = 0; ctr < bytes.Length / 2; ctr++)
            {
                temp = bytes[ctr];
                bytes[ctr] = bytes[highCtr];
                bytes[highCtr] = temp;
                highCtr -= 1;
            }
            return bytes;
        }

        public static short ToInt16(this byte[] bytes, int index = 0)
        {
            return (short)(bytes[0 + index] << 0 | bytes[1 + index] << 8);
        }

        public static int ToInt32(this byte[] bytes, int index = 0)
        {
            return (bytes[0 + index] << 0 | bytes[1 + index] << 8 | bytes[2 + index] << 16 | bytes[3 + index] << 24);
        }

        public static string ToString(this byte[] bytes, int offset, int count = -1)
        {
            try
            {
                if (count == -1)
                {
                    count = bytes.Length - offset;
                }
                char[] chars = new char[count];
                Encoding.UTF8.GetDecoder().Convert(bytes, offset, count, chars, 0, count, true, out int bytesUsed, out int charsUsed, out bool completed);
                return new string(chars);
            }
            catch
            {
                throw;
            }
        }
    }
}