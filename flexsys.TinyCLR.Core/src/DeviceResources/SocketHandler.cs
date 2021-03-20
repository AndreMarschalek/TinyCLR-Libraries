using System;
using System.Collections;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace flexsys.TinyCLR.Core
{
    public sealed partial class DeviceResources
    {
        private const int MAX_TCP_SOCKETS = 32;
        private static int _TcpSockets = 0;

        private const int MAX_TCP_LISTENING_SOCKETS = 12;
        private static int _TcpListeningSockets = 0;

        private const int MAX_UDP_SOCKETS = 16;
        private static int _UdpSockets = 0;

        private static void CatchSocket(ProtocolType protocolType, bool listening)
        {
            switch (protocolType)
            {
                case ProtocolType.Tcp:
                    while (_TcpSockets == MAX_TCP_SOCKETS)
                    {
                        Thread.Sleep(_Random.Next(100));
                    }

                    _TcpSockets++;

                    if (listening)
                    {
                        while (_TcpListeningSockets == MAX_TCP_LISTENING_SOCKETS)
                        {
                            Thread.Sleep(_Random.Next(100));
                        }
                        _TcpListeningSockets++;
                    }
                    break;

                case ProtocolType.Udp:
                    while (_UdpSockets == MAX_UDP_SOCKETS)
                    {
                        Thread.Sleep(_Random.Next(100));
                    }

                    _UdpSockets++;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private static void Print(int tcp_old, int tcpl_old, int udp_old)
        {
            Debug.WriteLine(
                "\r\n\tTCP_SOCKETS           : " + tcp_old + "\t-> " + _TcpSockets +
                "\r\n\tTCP_LISTENING_SOCKETS : " + tcpl_old + "\t-> " + _TcpListeningSockets +
                "\r\n\tUDP_SOCKETS           : " + udp_old + "\t-> " + _UdpSockets);
        }

        public sealed class SocketHandler : ServiceExtensions, IDisposable
        {
            public Socket Socket { get; set; }

            private readonly ProtocolType _ProtocolType;
            private readonly bool _Listening;
            private readonly string _Owner;

            public SocketHandler(string owner, AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, bool listening)
            {
                DebugPrint(this, owner + " new Instance " + listening);

                int tcp_old = _TcpSockets;
                int tcpl_old = _TcpListeningSockets;
                int udp_old = _UdpSockets;

                _Owner = owner;
                CatchSocket(protocolType, listening);
                _ProtocolType = protocolType;
                _Listening = listening;

                Socket = new Socket(addressFamily, socketType, protocolType);
                Print(tcp_old, tcpl_old, udp_old);
            }

            public void Dispose()
            {
                DebugPrint(this, _Owner + " Dispose " + _Listening);

                int tcp_old = _TcpSockets;
                int tcpl_old = _TcpListeningSockets;
                int udp_old = _UdpSockets;

                if (Socket != null)
                {
                    Socket.Close();
                    Socket = null;

                    switch (_ProtocolType)
                    {
                        case ProtocolType.Tcp:
                            _TcpSockets--;

                            if (_Listening)
                            {
                                _TcpListeningSockets--;
                            }
                            break;

                        case ProtocolType.Udp:
                            _UdpSockets--;
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
                Print(tcp_old, tcpl_old, udp_old);
            }
        }
    }
}