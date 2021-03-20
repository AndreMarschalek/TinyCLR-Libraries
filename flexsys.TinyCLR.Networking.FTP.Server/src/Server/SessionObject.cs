using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using flexsys.TinyCLR.Core;

namespace flexsys.TinyCLR.Networking.FTP.Server
{
    public class SessionObject : ServiceExtensions, IDisposable
    {
        internal SessionObject()
        {
        }

        public bool IsAlive { get; set; } = true;

        public bool IsAuthenticated { get; set; }

        public bool IsDataSocketConnected { get; set; }

        public bool IsPORT { get; set; }

        public int PBSZ { get; set; } = -1;

        public string PROT { get; set; } = "C";

        public IPEndPoint RemoteEndPoint { get; set; }

        public string RNFR { get; set; }

        public string User { get; set; }

        private string _WorkingDirectory;
        public string WorkingDirectory
        {
            get => _WorkingDirectory;
            set
            {
                _WorkingDirectory = value;
                DebugPrint(this, "Set WorkingDirectory to " + _WorkingDirectory);
            }
        }

        internal long Marker { get; set; }  // REST <SP> <marker> <CRLF>

        internal DeviceResources.SocketHandler SHC { get; set; }

        internal DeviceResources.SocketHandler SHD { get; set; }

        internal NetworkStream ControlStream { get; set; }

        internal NetworkStream DataStream { get; set; }

        internal DateTime TimeOut { get; set; }

        public void DisposeSHD()
        {
            if (DataStream != null)
            {
                DataStream.Close();
                DataStream.Dispose();
                DataStream = null;
            }
            if (SHD != null)
            {
                SHD.Dispose();
            }
        }

        public void Dispose()
        {
            DisposeSHD();

            if (ControlStream != null)
            {
                ControlStream.Close();
                ControlStream.Dispose();
                ControlStream = null;
            }
            SHC.Dispose();
            DebugPrint(this, "FTP.SessionObject disposed");
        }
    }
}
