/*
    RFC 959 - FILE TRANSFER PROTOCOL (FTP)
    http://www.ietf.org/rfc/rfc959.txt

    RFC 1579 - Firewall-Friendly FTP
    http://tools.ietf.org/rfc/rfc1579.txt

    RFC 2228 - FTP Security Extensions
    http://tools.ietf.org/rfc/rfc2228.txt

    RFC 2389 - Feature negotiation mechanism for the File Transfer Protocol
    http://tools.ietf.org/rfc/rfc2389.txt

    RFC 2428 - FTP Extensions for IPv6 and NATs
    http://tools.ietf.org/rfc/rfc2428.txt

    RFC 2640 - Internationalization of the File Transfer Protocol
    http://tools.ietf.org/rfc/rfc2640.txt

    RFC 3659 - Extensions to FTP
    http://www.ietf.org/rfc/rfc3659.txt

    RFC 4217 - Securing FTP with TLS
    http://www.ietf.org/rfc/rfc4217.txt

    RFC 5797 - FTP Command and Extension Registry
    http://www.ietf.org/rfc/rfc4217.txt
*/

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using flexsys.TinyCLR.Core;
using flexsys.TinyCLR.Networking.FTP.Server;

namespace flexsys.TinyCLR.Service
{
    public class FTP : ServiceExtensions
    {
        public ThreadState GetThreadState => _Thread.ThreadState;

        private readonly Service _Service;
        private readonly Thread _Thread;

        public FTP(Configuration configuration)
        {
            Configuration = configuration;

            _Service = new Service();
            _Thread = new Thread(_Service.Start);
        }

        public void Start()
        {
            _Thread.Start();
        }

        public void Stop()
        {
            _Service.Stop();
            if (_Thread.ThreadState != ThreadState.Unstarted)
            {
                _Thread.Join();
            }
        }

        public bool IsDebugPrintEnabled
        {
            get => DebugPrintEnabled;
            set
            {
                DebugPrintEnabled = value;
                if (_Service != null)
                {
                    _Service.DebugPrintEnabled = value;
                }
            }
        }

        internal static Configuration Configuration { get; set; }

        internal static Hashtable Blocked;
        internal static Hashtable IPS;
        internal static Hashtable Sessions;

        private class Service : ServiceExtensions
        {
            private int shutdown = 0;
            private readonly byte[] WelcomeMessage;

            public Service()
            {
                IPS = new Hashtable();
                Blocked = new Hashtable();
                Sessions = new Hashtable();
                WelcomeMessage = Encoding.UTF8.GetBytes("220 " + Configuration.WelcomeMessage + "\r\n");
            }

            public void Start()
            {
                DeviceResources.Instance.NetworkReady.WaitOne();
                while (shutdown == 0)
                {
                    try
                    {
                        using (DeviceResources.SocketHandler SH = new DeviceResources.SocketHandler("FTP.Service.Start Server", AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, true))
                        {
                            if (Configuration.ServerPort == 0)
                            {
                                Configuration.ServerPort = Configuration.Certificate == null || Configuration.Method == Method.Explicit ? 21 : 990;
                            }

                            SH.Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                            SH.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                            //SH.NSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);
                            SH.Socket.Bind(new IPEndPoint(IPAddress.Any, Configuration.ServerPort));
                            SH.Socket.Listen(Configuration.Backlog);

                            SessionObject Session;

                            while (shutdown == 0)
                            {
                                // waiting for clients

                                Session = new SessionObject
                                {
                                    SHC = new DeviceResources.SocketHandler("FTP.Service.Start Session", AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, false)
                                };

                                Session.SHC.Socket = SH.Socket.Accept();
                                Session.SHC.Socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                                //Session.SHC.NSocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.KeepAlive, true);   // 500 STOR command failed, Critical file transfer error ?

                                Session.RemoteEndPoint = (IPEndPoint)Session.SHC.Socket.RemoteEndPoint;

                                IPAddress ip = Session.RemoteEndPoint.Address;

                                if (Blocked.Contains(ip))
                                {
                                    IPSObject obj = (IPSObject)Blocked[ip];
                                    if (DateTime.Now < obj.TimeStamp.AddMinutes(Configuration.IpsBlockTime))
                                    {
                                        Session.Dispose();
                                        Session = null;
                                        continue;
                                    }
                                    else
                                    {
                                        Blocked.Remove(ip);
                                    }
                                }

                                if (IPS.Contains(ip))
                                {
                                    IPSObject obj = (IPSObject)IPS[ip];
                                    obj.count++;
                                    if (DateTime.Now < obj.TimeStamp.AddMinutes(1) && obj.count == Configuration.MaxConnectionsPerMinute)
                                    {
                                        IPS.Remove(ip);
                                        Blocked.Add(ip, new IPSObject());
                                        Session.Dispose();
                                        Session = null;
                                        continue;
                                    }
                                }
                                else
                                {
                                    IPS.Add(ip, new IPSObject());
                                }

                                if (Configuration.Certificate == null || Configuration.Method == Method.Explicit)
                                {
                                    Session.ControlStream = new NetworkStream(Session.SHC.Socket, true);
                                }
                                else
                                {
                                    SslStream sslStream = new SslStream(Session.SHC.Socket);

                                    DateTime timeout = DateTime.Now.AddSeconds(20);
                                    while (DateTime.Now.CompareTo(timeout) <= 0)
                                    {
                                        try
                                        {
                                            sslStream.AuthenticateAsServer(Configuration.Certificate, System.Security.Authentication.SslProtocols.Tls12);

                                            Session.ControlStream = sslStream;

                                            break;
                                        }
                                        catch (InvalidOperationException)
                                        {
                                        }
                                    }
                                }

                                if (Session.ControlStream != null)
                                {
                                    Session.ControlStream.Write(WelcomeMessage, 0, WelcomeMessage.Length);

                                    // handle the client in a new thread

                                    ClientHandler Client = new ClientHandler(Session, DebugPrintEnabled);
                                    Thread clientThread = new Thread(Client.Start);
                                    clientThread.Start();
                                }
                                else
                                {
                                    Session.SHC.Dispose();
                                }
                            }
                        }
                    }
                    catch
                    {
                        ErrorPrint(this, "FTP EXCEPTION");
                    }
                }
            }

            public void Stop()
            {
                Interlocked.Exchange(ref shutdown, 1);
            }

            protected class ClientHandler : ServiceExtensions
            {
                private readonly Command cmd;
                private readonly Communication com;
                private SessionObject Session;
                private int shutdown;

                public ClientHandler(SessionObject Session, bool DebugPrintEnabled)
                {
                    this.Session = Session;
                    this.DebugPrintEnabled = DebugPrintEnabled;
                    shutdown = 0;
                    cmd = new Command();
                    com = new Communication();
                }

                public void Start()
                {
                    cmd.DebugPrintEnabled = this.DebugPrintEnabled;
                    com.DebugPrintEnabled = this.DebugPrintEnabled;
                    using (Session)
                    {
                        while (shutdown == 0)
                        {
                            if (Session.SHC.Socket.Poll(5000000, SelectMode.SelectRead))
                            {
                                // If 0 bytes in buffer, then the connection has been closed, reset, or terminated.
                                if (Session.SHC.Socket.Available == 0)
                                {
                                    break;
                                }

                                byte[] BinaryData = ReceiveClientData;

                                if (BinaryData == null)
                                {
                                    break;
                                }

                                RequestObject Request = Networking.FTP.Server.FTP.Parse(BinaryData);

                                // process commands
                                if (Session.IsAlive && Request != null && Request.Command != null)
                                {
                                    DebugPrint(this, "C " + Request.Command + " " + Request.Argument);

                                    if (!Session.IsAuthenticated)
                                    {
                                        if (Request.Command != "AUTH" && Request.Command != "USER" && Request.Command != "PASS")
                                        {
                                            com.SendControl(Session, "530 Please login with USER and PASS.");
                                            return;
                                        }
                                        if (Request.Command == "PASS" && Session.User == string.Empty)
                                        {
                                            com.SendControl(Session, "503 Login with USER first.");
                                            return;
                                        }
                                        switch (Request.Command)
                                        {
                                            case "AUTH":
                                                cmd.AUTH(ref Session, com, Request.Argument);
                                                break;

                                            case "USER":
                                                cmd.USER(ref Session, com, Request.Argument);
                                                break;

                                            case "PASS":
                                                if (Session.User == "anonymous")
                                                {
                                                    com.SendControl(Session, "530 Anonymous Authentication is not allowed.");
                                                    Session.IsAlive = false;
                                                    break;
                                                }
                                                if (Session.User != "anonymous")
                                                {
                                                    bool found = false;

                                                    DictionaryEntry de;
                                                    for (int i = 0; i < Sessions.Count; i++)
                                                    {
                                                        de = (DictionaryEntry) Sessions[i];
                                                        
                                                        if (Request.Argument == de.Value.ToString())
                                                        {
                                                            found = true;
                                                            break;
                                                        }
                                                    }
                                                    //foreach (DictionaryEntry de in Sessions)
                                                    //{
                                                    //    if (Request.Argument == de.Value.ToString())
                                                    //        found = true;
                                                    //}
                                                    if (found)
                                                    {
                                                        com.SendControl(Session, "530 '" + Request.Argument + "' already logged in.");
                                                        Session.IsAlive = false;
                                                        break;
                                                    }
                                                }
                                                cmd.PASS(ref Session, com, Request);
                                                if (!Session.IsAuthenticated)
                                                {
                                                    IPSObject obj = (IPSObject)IPS[Session.RemoteEndPoint.Address];
                                                    obj.fault++;
                                                    obj.TimeStamp = DateTime.Now;
                                                    if (obj.fault == Configuration.MaxWrongPasswords)
                                                    {
                                                        IPS.Remove(Session.RemoteEndPoint.Address);
                                                        Blocked.Add(Session.RemoteEndPoint.Address, obj);
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    IPS.Remove(Session.RemoteEndPoint.Address);
                                                    DebugPrint(this, "Add " + (Session.ControlStream is SslStream ? "ftps" : "ftp") + " Session with " + (Session.User == string.Empty ? "anonymous" : Session.User) + "@" + Session.RemoteEndPoint.Address + ":" + Session.RemoteEndPoint.Port);
                                                    Sessions.Add(Session.RemoteEndPoint, Session.User);
                                                }
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        switch (Request.Command)
                                        {
                                            case "APPE":
                                                cmd.APPE(ref Session, com, Request.Argument);
                                                break;
                                            case "AUTH":
                                                cmd.AUTH(ref Session, com, Request.Argument);
                                                break;
                                            case "CDUP":
                                            case "XCUP":
                                                cmd.CDUP(ref Session, com);
                                                break;
                                            case "CWD":
                                            case "CD":
                                            case "XCWD":
                                                cmd.CWD(ref Session, com, Request.Argument);
                                                break;
                                            case "DELE":
                                                cmd.DELE(ref Session, com, Request.Argument);
                                                break;
                                            case "EPSV":    // todo: parameters are not recognized
                                                cmd.EPSV(ref Session, com);
                                                break;
                                            case "FEAT":
                                                cmd.FEAT(ref Session, com);
                                                break;
                                            case "HELP":
                                                cmd.HELP(ref Session, com);
                                                break;
                                            case "LIST":
                                                cmd.LIST(ref Session, com, Request.Argument);
                                                break;
                                            case "MDTM":
                                                cmd.MDTM(ref Session, com, Request.Argument);
                                                break;
                                            case "MKD":
                                            case "XMKD":
                                                cmd.MKD(ref Session, com, Request.Argument);
                                                break;
                                            case "MLSD":
                                                cmd.MLSD(ref Session, com, Request.Argument);
                                                break;
                                            case "MLST":
                                                cmd.MLST(ref Session, com, Request.Argument);
                                                break;
                                            case "NLST":
                                                cmd.NLST(ref Session, com);
                                                break;
                                            case "NOOP":
                                                cmd.NOOP(ref Session, com);
                                                break;
                                            case "PASV":
                                                cmd.PASV(ref Session, com);
                                                break;
                                            case "PBSZ":
                                                cmd.PBSZ(ref Session, com, Request.Argument);
                                                break;
                                            case "PORT":
                                                cmd.PORT(ref Session, com, Request.Argument);
                                                break;
                                            case "PROT":
                                                cmd.PROT(ref Session, com, Request.Argument);
                                                break;
                                            case "PWD":
                                            case "XPWD":
                                                cmd.PWD(ref Session, com);
                                                break;
                                            case "QUIT":
                                                cmd.QUIT(ref Session, com);
                                                break;
                                            case "REST":
                                                cmd.REST(ref Session, com, Request.Argument);
                                                break;
                                            case "RETR":
                                                cmd.RETR(ref Session, com, Request.Argument);
                                                break;
                                            case "RMD":
                                            case "XRMD":
                                                cmd.RMD(ref Session, com, Request.Argument);
                                                break;
                                            case "RNFR":
                                                cmd.RNFR(ref Session, com, Request.Argument);
                                                break;
                                            case "RNTO":
                                                cmd.RNTO(ref Session, com, Request.Argument);
                                                break;
                                            case "SIZE":
                                                cmd.SIZE(ref Session, com, Request.Argument);
                                                break;
                                            case "STOR":
                                                cmd.STOR(ref Session, com, Request.Argument);
                                                break;
                                            case "SYST":
                                                cmd.SYST(ref Session, com);
                                                break;
                                            case "TYPE":
                                                cmd.TYPE(ref Session, com, Request.Argument);
                                                break;
                                            default:
                                                com.SendControl(Session, "502 " + Request.Command + " not implemented.");
                                                break;
                                        }
                                    }
                                }
                            }
                            if (DateTime.Now.CompareTo(Session.TimeOut) > 0)
                            {
                                shutdown = 1;
                            }
                        }
                        DebugPrint(this, "Remove " + (Session.ControlStream is SslStream ? "ftps" : "ftp") + " Session with " + (Session.User == string.Empty ? "anonymous" : Session.User) + "@" + Session.RemoteEndPoint.Address + ":" + Session.RemoteEndPoint.Port);
                        Sessions.Remove(Session.RemoteEndPoint);
                    }
                }

                public void Stop()
                {
                    Interlocked.Exchange(ref shutdown, 1);
                }

                private byte[] ReceiveClientData
                {
                    get
                    {
                        DateTime timeout = DateTime.Now.AddSeconds(20);
                        MemoryStream mStream = new MemoryStream();
                        int read;
                        byte[] buffer;

                        while (Session.SHC.Socket.Available == 0)
                        {
                            if (DateTime.Now.CompareTo(timeout) > 0)
                            {
                                return null;
                            }

                            Thread.Sleep(100);
                        }
                        while (Session.SHC.Socket.Available > 0)
                        {
                            while (Session.SHC.Socket.Available > 0)
                            {
                                buffer = new byte[Session.SHC.Socket.Available];
                                read = Session.ControlStream.Read(buffer, 0, buffer.Length);
                                if (read > 0)
                                {
                                    mStream.Write(buffer, 0, read);
                                    //DebugPrint(this, "BytesReceived: " + mStream.Length);
                                }
                            }
                            Thread.Sleep(100); // sometimes Socket.Available is 0 but not all bytes where received
                        }
                        Session.TimeOut = DateTime.Now.AddMinutes(Configuration.SessionTimeout);

                        return mStream.ToArray();
                    }
                }
            }
        }
    }
}
