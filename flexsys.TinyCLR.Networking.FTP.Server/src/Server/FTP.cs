using System;
using System.Collections;
using System.Text;
using System.Threading;
using flexsys.TinyCLR.Core.Extensions;

namespace flexsys.TinyCLR.Networking.FTP.Server
{
    internal class FTP
    {
        public static RequestObject Parse(byte[] data)
        {
            if (data == null)
                return null;

            if (data.Length == 0)
                return null;

            RequestObject Request = new RequestObject();

            int CommandLength = Search(data, Encoding.UTF8.GetBytes("\r\n"), 0);

            if (CommandLength < 1)
                return null;

            byte[] ByteCommand = new byte[CommandLength];
            Array.Copy(data, 0, ByteCommand, 0, CommandLength);

            Request.RawCommand = ByteCommand.ToString(0);

            if (Request.RawCommand == string.Empty)
                throw new Exception("malformed command");

            if (Request.RawCommand.Length == 3 || Request.RawCommand.Length == 4)
            {
                Request.Command = Request.RawCommand.ToUpper();
                Request.Argument = string.Empty;
            }
            else
            {
                Request.Command = Request.RawCommand.Substring(0, Request.RawCommand.IndexOf(' ')).ToUpper();
                Request.Argument = Request.RawCommand.Substring(Request.RawCommand.IndexOf(' ') + 1);
            }
            return Request;
        }

        private static int Search(byte[] source, byte[] search, int start = 0)
        {
            for (int i = start; i < source.Length; i++)
            {
                bool found = true;
                for (int s = 0; s < search.Length; s++)
                {
                    if (source[i + s] != search[s])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
