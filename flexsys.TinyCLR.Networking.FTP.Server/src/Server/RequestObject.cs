using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace flexsys.TinyCLR.Networking.FTP.Server
{
    public class RequestObject
    {
        public string Argument { get; set; }

        public string Command { get; set; }

        public string RawCommand { get; set; }
    }
}
