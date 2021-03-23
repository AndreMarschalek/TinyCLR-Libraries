/*
    RFC4226 - HOTP: An HMAC-Based One-Time Password Algorithm
    http://tools.ietf.org/html/rfc4226

    RFC6238 - TOTP: Time-Based One-Time Password Algorithm
    http://tools.ietf.org/html/rfc6238
*/

using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using GHIElectronics.TinyCLR.Drivers.Barcode;
using GHIElectronics.TinyCLR.Drivers.Barcode.Common;
using GHIElectronics.TinyCLR.Drivers.Barcode.QrCode;
using Crypto = flexsys.TinyCLR.Cryptography.Crypto;

namespace flexsys.TinyCLR.Authentication
{
    public class OTP
    {
        private const byte DIGITS = 6;

        private readonly int[] DIGITS_POWER = { 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000 };

        private const long EPOCH = 621355968000000000;

        private const int INTERVAL = 30000;

        public string Key
        {
            set
            {
                _sKey = value;
                _bKey = ToBytesBase32(value);
            }
        }

        public string TOTP => Generate();

        public bool IsValid(string totp) => (totp.Equals(Generate()));

        private byte[] _bKey { get; set; }

        private string _sKey { get; set; }

        private long TimeSource => (DateTime.UtcNow.Ticks - EPOCH) / TimeSpan.TicksPerMillisecond;

        public OTP()
        {
        }

        public Bitmap QRCode(string label, int width = 150, int height = 150)
        {
            QRCodeWriter writer = new QRCodeWriter();
            BitMatrix matrix = writer.encode("otpauth://totp/" + label + "?secret=" + _sKey, BarcodeFormat.QR_CODE, width, height);
            return matrix.ToBitmap(BarcodeFormat.QR_CODE, null);
        }

        private byte[] HmacSha1(byte[] data)
        {
            Crypto.Digests.Sha1Digest sha1 = new Crypto.Digests.Sha1Digest();
            Crypto.Macs.HMac hmac = new Crypto.Macs.HMac(sha1);
            hmac.Init(new Crypto.Parameters.KeyParameter(_bKey));
            byte[] result = new byte[hmac.GetMacSize()];
            lock (hmac)
            {
                hmac.BlockUpdate(data, 0, data.Length);
                hmac.DoFinal(result, 0);
            }
            return result;
        }

        private string Generate()
        {
            byte[] code = BitConverter.GetBytes(TimeSource / INTERVAL);

            if (BitConverter.IsLittleEndian)
            {
                code = Reverse(code);
            }

            //string message = "code : ";
            //for (int i = 0; i < code.Length; i++)
            //{
            //    message += code[i].ToString("X2");
            //}
            //Debug.WriteLine(message);

            byte[] hash = HmacSha1(code);

            int offset = hash[hash.Length - 1] & 0xf;

            int binary =
                ((hash[offset] & 0x7f) << 24) |
                ((hash[offset + 1] & 0xff) << 16) |
                ((hash[offset + 2] & 0xff) << 8) |
                (hash[offset + 3] & 0xff);

            int otp = binary % DIGITS_POWER[DIGITS];

            string result = otp.ToString();
            while (result.Length < DIGITS)
            {
                result = "0" + result;
            }
            return result;
        }

        private byte[] Reverse(byte[] source)
        {
            byte[] reversed = new byte[source.Length];

            for (int i = 0; i < source.Length; i++)
            {
                reversed[i] = source[source.Length - 1 - i];
            }

            return reversed;
        }

        private byte[] ToBytesBase32(string source)
        {
            source = source.TrimEnd('=');
            int byteCount = source.Length * 5 / 8;
            byte[] returnArray = new byte[byteCount];

            byte curByte = 0, bitsRemaining = 8;
            int mask, arrayIndex = 0;

            char[] chars = source.ToUpper().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                int cValue = CharToValue(chars[i]);

                if (bitsRemaining > 5)
                {
                    mask = cValue << (bitsRemaining - 5);
                    curByte = (byte)(curByte | mask);
                    bitsRemaining -= 5;
                }
                else
                {
                    mask = cValue >> (5 - bitsRemaining);
                    curByte = (byte)(curByte | mask);
                    returnArray[arrayIndex++] = curByte;
                    curByte = (byte)(cValue << (3 + bitsRemaining));
                    bitsRemaining += 3;
                }
            }

            if (arrayIndex != byteCount)
            {
                returnArray[arrayIndex] = curByte;
            }

            return returnArray;
        }

        private int CharToValue(int c)
        {
            if (c < 91 && c > 64)
            {
                return c - 65;
            }
            if (c < 56 && c > 49)
            {
                return c - 24;
            }
            if (c < 123 && c > 96)
            {
                return c - 97;
            }

            return -1;
        }
    }
}
