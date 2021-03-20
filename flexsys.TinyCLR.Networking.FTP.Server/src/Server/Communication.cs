using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using flexsys.TinyCLR.Core;

namespace flexsys.TinyCLR.Networking.FTP.Server
{
    internal class Communication : ServiceExtensions
    {
        public Communication()
        {
        }

        public void Disconnect(ref SessionObject Session)
        {
            Session.DisposeSHD();
        }

        internal byte[] Bind(ref SessionObject Session)
        {
            try
            {
                Session.SHD = new DeviceResources.SocketHandler("FTP.Server.Bind", AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, true);
                Session.SHD.Socket.Bind(new IPEndPoint(DeviceInformation.LocalAddress, 0));
                Session.SHD.Socket.Listen(1);
                Session.IsDataSocketConnected = true;
                short value = (short)((IPEndPoint)Session.SHD.Socket.LocalEndPoint).Port;
                return new byte[2] { (byte)(value & 0xFF), (byte)((value >> 8) & 0xFF) };
            }
            catch (SocketException ex)
            {
                ErrorPrint(this, ex.ErrorCode, ex.Message, ex.StackTrace);
                Disconnect(ref Session);
                return null;
            }
        }

        internal bool Connect(ref SessionObject Session, EndPoint pEndPoint)
        {
            try
            {
                Session.SHD = new DeviceResources.SocketHandler("FTP.Server.Connect", AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, false);
                Session.SHD.Socket.Connect(pEndPoint);
                Session.IsDataSocketConnected = true;
                return true;
            }
            catch (SocketException ex)
            {
                ErrorPrint(this, ex.ErrorCode, ex.Message, ex.StackTrace);
                Disconnect(ref Session);
                return false;
            }
        }

        internal void Receive(ref SessionObject Session, string pCommand, string pPath, FileMode pFileMode)
        {
            Debug.WriteLine("Receive");
            DateTime timeout;
            Socket socket;
            try
            {
                SendControl(Session, "150 About to open data connection.");
                
                socket = Session.IsPORT ? Session.SHD.Socket : Session.SHD.Socket.Accept();   // new socket connection?

                if (Session.PROT == "C")
                {
                    Session.DataStream = new NetworkStream(socket, true);
                }
                else
                {
                    SslStream sslStream = new SslStream(socket);

                    timeout = DateTime.Now.AddSeconds(20);
                    while (DateTime.Now.CompareTo(timeout) <= 0)
                    {
                        try
                        {
                            sslStream.AuthenticateAsServer(Service.FTP.Configuration.Certificate, System.Security.Authentication.SslProtocols.Tls12);

                            Session.DataStream = sslStream;

                            break;
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
                }

                Receive(ref Session, socket, pPath, pFileMode);

                SendControl(Session, "226 Transfer completed successfully; Closing data connection.");
            }
            catch (SocketException ex)
            {
                ErrorPrint(this, ex.ErrorCode, ex.Message, ex.StackTrace);
                SendControl(Session, "500 " + pCommand + " command failed.");
            }
            catch (Exception ex)
            {
                ErrorPrint(this, -1, ex.Message, ex.StackTrace);
                SendControl(Session, "500 " + pCommand + " command failed.");
            }
            finally
            {
                Disconnect(ref Session);
            }
            Debug.WriteLine("/Receive");
        }

        internal void SendControl(SessionObject Session, string pOut)
        {
            DebugPrint(this, "S " + pOut);
            byte[] buffer = Encoding.UTF8.GetBytes(pOut + "\r\n");
            Session.ControlStream.Write(buffer, 0, buffer.Length);
        }

        internal void SendData(ref SessionObject Session, string pCommand, string pOut)
        {
            SendOverDataChannel(ref Session, pCommand, Encoding.UTF8.GetBytes(pOut));
        }

        internal void SendData(ref SessionObject Session, string pCommand, FileStream fStream)
        {
            SendOverDataChannel(ref Session, pCommand, fStream);
        }

        private protected void SendOverDataChannel(ref SessionObject Session, string pCommand, object pOut)
        {
            Socket socket = null;
            try
            {
                SendControl(Session, "150 About to open data connection.");

                socket = Session.IsPORT ? Session.SHD.Socket : Session.SHD.Socket.Accept();

                if (Session.PROT == "C")
                {
                    if (pOut is byte[] data)
                    {
                        socket.Send(data);
                        DebugPrint(this, "BytesSent: " + data.Length);
                    }
                    else
                    {
                        FileStream fstream = pOut as FileStream;
                        Session.DataStream = new NetworkStream(socket, true);
                        Send(Session.DataStream, fstream);

                        //Session.DataStream.Flush();
                        Session.DataStream.Close();
                        Session.DataStream.Dispose();
                        Session.DataStream = null;
                    }
                }
                else
                {
                    SslStream sslStream = new SslStream(socket);

                    DateTime timeout = DateTime.Now.AddSeconds(20);
                    while (DateTime.Now.CompareTo(timeout) <= 0)
                    {
                        try
                        {
                            sslStream.AuthenticateAsServer(Service.FTP.Configuration.Certificate, System.Security.Authentication.SslProtocols.Tls12);

                            Session.DataStream = sslStream;

                            break;
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }

                    if (Session.DataStream != null)
                    {
                        if (pOut is byte[] data)
                        {
                            Session.DataStream.Write(data, 0, data.Length);
                            DebugPrint(this, "BytesSent: " + data.Length);
                        }
                        else
                        {
                            FileStream fstream = pOut as FileStream;
                            Send(Session.DataStream, fstream);
                        }
                        
                        //Session.DataStream.Flush();
                        Session.DataStream.Close();
                        Session.DataStream.Dispose();
                        Session.DataStream = null;
                    }
                }
                SendControl(Session, "226 Transfer completed successfully; Closing data connection.");
            }
            catch (SocketException ex)
            {
                ErrorPrint(this, ex.ErrorCode, ex.Message, ex.StackTrace);
                SendControl(Session, "500 " + pCommand + " command failed.");
            }
            catch (Exception ex)
            {
                ErrorPrint(this, -1, ex.Message, ex.StackTrace);
                SendControl(Session, "500 " + pCommand + " command failed.");
            }
            finally
            {
                Session.IsPORT = false;
                if (socket != null)
                {
                    socket.Close();
                }
                Disconnect(ref Session);
            }
        }

        private protected void Receive(ref SessionObject session, Socket socket, string pPath, FileMode pFileMode)
        {
            try
            {
                int read;
                byte[] buffer = new byte[1024 * 4]; // todo: define size
                DateTime timeout = DateTime.Now.AddSeconds(30);

                using (DeviceResources.FileHandler FH = new DeviceResources.FileHandler(pPath, pFileMode, FileAccess.Write))
                {
                    if (session.Marker != 0)
                    {
                        FH.FStream.Seek(session.Marker, SeekOrigin.Begin);
                        session.Marker = 0;
                    }

                    while (socket.Available == 0)
                    {
                        if (DateTime.Now.CompareTo(timeout) > 0)
                        {
                            throw new Exception("timeout");
                        }

                        Thread.Sleep(100);
                    }

                    while (socket.Available > 0)
                    {
                        while (socket.Available > 0)
                        {
                            read = session.DataStream.Read(buffer, 0, buffer.Length);
                            if (read > 0)
                            {
                                FH.FStream.Write(buffer, 0, read);
                                DebugPrint(this, "BytesReceived: " + FH.FStream.Position);
                            }
                        }
                        Thread.Sleep(100); // sometimes Socket.Available is 0 but not all bytes where received
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        private protected void Send(NetworkStream nStream, FileStream fStream)
        {
            int length;
            byte[] buffer = new byte[1024 * 4]; // todo: define size

            using (StreamReader reader = new StreamReader(fStream))
            {
                while (reader.BaseStream.Position != fStream.Length) 
                {
                    length = reader.BaseStream.Read(buffer, 0, buffer.Length);
                    nStream.Write(buffer, 0, length);
                    DebugPrint(this, "BytesSent: " + reader.BaseStream.Position + " / " + fStream.Length);
                }
                nStream.Flush();
            }
        }
    }
}
