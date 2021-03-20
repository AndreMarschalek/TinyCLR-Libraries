using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace flexsys.TinyCLR.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool Contains(this string source, string check)
        {
            // check string must be smaller
            try
            {
                if (check.Length > source.Length)
                {
                    return false;
                }

                // Now do the easy check
                if (source == check)
                {
                    return true;
                }

                for (int i = 0; i < source.Length; i++)
                {
                    if (source.Substring(i, check.Length) == check)
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        public static bool EndsWith(this string source, string check)
        {
            if (source.Length < check.Length)
            {
                return false;
            }

            return (source.Substring(source.Length - check.Length, check.Length) == check);
        }

        public static byte[] GetBytes(this string source)
        {
            try
            {
                return Encoding.UTF8.GetBytes(source);
            }
            catch
            {
                throw;
            }
        }

        public static bool StartsWith(this string source, string check)
        {
            if (check == null || source.Length < check.Length)
            {
                return false;
            }

            return (source.Substring(0, check.Length) == check);
        }
    }
}
