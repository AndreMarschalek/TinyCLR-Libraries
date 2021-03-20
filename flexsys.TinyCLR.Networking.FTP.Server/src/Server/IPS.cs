using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace flexsys.TinyCLR.Networking.FTP.Server
{
    internal class IPSObject
    {
        internal int count;
        internal int fault;
        internal DateTime TimeStamp;

        public IPSObject()
        {
            TimeStamp = DateTime.Now;
            count = 1;
        }
    }
}
