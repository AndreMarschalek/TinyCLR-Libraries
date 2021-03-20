using System;
using System.Collections;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace flexsys.TinyCLR.Core
{
    public sealed partial class DeviceResources : ServiceExtensions
    {
        public static DeviceResources Instance { get; } = new DeviceResources();

        public ManualResetEvent DatabaseReady = new ManualResetEvent(false);
        public ManualResetEvent FileSystemReady = new ManualResetEvent(false);
        public ManualResetEvent NetworkReady = new ManualResetEvent(false);
        
        private static readonly Random _Random = new Random();

        DeviceResources()
        {
        }



        //#region FILEHANDLER

        //private const int MAX_OPEN_FILEHANDLES = 8;
        //private static int _OpenFileHandles = 0;

        //public class FileHandler : ServiceExtensions, IDisposable
        //{
        //    public FileStream FStream { get; set; }

        //    public FileHandler()
        //    {
        //        while (_OpenFileHandles == MAX_OPEN_FILEHANDLES)
        //        {
        //            Thread.Sleep(_Random.Next(100));
        //        }
        //        _OpenFileHandles++;
        //        Print();
        //    }

        //    public FileHandler(string path, FileMode mode, FileAccess access, FileShare share = FileShare.None)
        //    {
        //        DebugPrint(this, string.Empty);
        //        while (_OpenFileHandles == MAX_OPEN_FILEHANDLES)
        //        {
        //            Thread.Sleep(_Random.Next(100));
        //        }
        //        _OpenFileHandles++;
        //        FStream = new FileStream(path, mode, access, share);
        //        Print();
        //    }

        //    private void Print()
        //    {
        //        DebugPrint(this, "\r\n\tOPEN_FILEHANDLES      : " + _OpenFileHandles);
        //    }

        //    public void Dispose()
        //    {
        //        if (FStream != null)
        //        {
        //            FStream.Flush();
        //            FStream.Close();
        //            FStream.Dispose();
        //            FStream = null;
        //        }
        //        _OpenFileHandles--;
        //        Print();
        //    }
        //}

        //#endregion FILEHANDLER

        //#region SOCKETHANDLER

        //private const int MAX_TCP_SOCKETS = 32;
        //private static int _TcpSockets = 0;

        //private const int MAX_TCP_LISTENING_SOCKETS = 12;
        //private static int _TcpListeningSockets = 0;

        //private const int MAX_UDP_SOCKETS = 16;
        //private static int _UdpSockets = 0;

        //public class SocketHandler : ServiceExtensions, IDisposable
        //{
        //    public Socket NSocket { get; set; }

        //    private readonly ProtocolType _ProtocolType;
        //    private readonly bool _Listening;

        //    public SocketHandler(ProtocolType protocolType, bool listening = false)
        //    {
        //        CatchSocket(protocolType, listening);
        //        _ProtocolType = protocolType;
        //        _Listening = listening;
        //        Print();
        //    }

        //    public SocketHandler(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType, bool listening = false)
        //    {
        //        CatchSocket(protocolType, listening);
        //        _ProtocolType = protocolType;
        //        _Listening = listening;
        //        NSocket = new Socket(addressFamily, socketType, protocolType);
        //        Print();
        //    }

        //    public void Dispose()
        //    {
        //        if (NSocket != null)
        //        {
        //            NSocket.Close();
        //            NSocket = null;
        //        }
        //        switch (_ProtocolType)
        //        {
        //            case ProtocolType.Tcp:
        //                _TcpSockets--;
        //                if (_Listening)
        //                {
        //                    _TcpListeningSockets--;
        //                }
        //                break;

        //            case ProtocolType.Udp:
        //                _UdpSockets--;
        //                break;

        //            default:
        //                throw new NotImplementedException();
        //        }
        //        Print();
        //    }

        //    private void CatchSocket(ProtocolType protocolType, bool listening)
        //    {
        //        switch (protocolType)
        //        {
        //            case ProtocolType.Tcp:
        //                while (_TcpSockets == MAX_TCP_SOCKETS)
        //                {
        //                    Thread.Sleep(_Random.Next(100));
        //                }
        //                _TcpSockets++;
        //                if (listening)
        //                {
        //                    while (_TcpListeningSockets == MAX_TCP_LISTENING_SOCKETS)
        //                    {
        //                        Thread.Sleep(_Random.Next(100));
        //                    }
        //                    _TcpListeningSockets++;
        //                }
        //                break;

        //            case ProtocolType.Udp:
        //                while (_UdpSockets == MAX_UDP_SOCKETS)
        //                {
        //                    Thread.Sleep(_Random.Next(100));
        //                }
        //                _UdpSockets++;
        //                break;

        //            default:
        //                throw new NotImplementedException();
        //        }
        //    }

        //    private void Print()
        //    {
        //        DebugPrint(this, 
        //            "\r\n\tTCP_SOCKETS           : " + _TcpSockets + 
        //            "\r\n\tTCP_LISTENING_SOCKETS : " + _TcpListeningSockets + 
        //            "\r\n\tUDP_SOCKETS           : " + _UdpSockets);
        //    }
        //}

        //#endregion SOCKETHANDLER

        //#region SQLite

        //public sealed class DB : ServiceExtensions
        //{
        //    public static DB Instance { get; } = new DB();

        //    private static SQLiteDatabase _Database;
        //    private FileHandler _FH;

        //    public DB()
        //    {
        //    }

        //    public void Dispose()
        //    {
        //        _Database.Dispose();
        //        _FH.Dispose();
        //    }

        //    public void Database_Open(string file)
        //    {
        //        _FH = new FileHandler();
        //        _Database = new SQLiteDatabase(file);
        //        //_Database.Open(@"\SD\system\database.db3");
        //        ResourceHandler.Instance.DatabaseReady.Set();
        //    }

        //    public ResultSet ExecuteQuery(string sql)
        //    {
        //        try
        //        {
        //            if (sql[sql.Length] != ';')
        //            {
        //                sql += ";";
        //            }
        //            DebugPrint(this, "\r\n\tSQL : " + sql);
        //            return _Database.ExecuteQuery(sql);
        //        }
        //        catch (Exception ex)
        //        {
        //            ErrorPrint(this, ex.Message);
        //            return null;
        //        }
        //    }

        //    public bool ExecuteNonQuery(string sql)
        //    {
        //        try
        //        {
        //            if (sql[sql.Length] != ';')
        //            {
        //                sql += ";";
        //            }
        //            DebugPrint(this, "\r\n\tSQL : " + sql);
        //            _Database.ExecuteNonQuery(sql);
        //            return true;
        //        }
        //        catch (Exception ex)
        //        {
        //            ErrorPrint(this, ex.Message);
        //            return false;
        //        }
        //    }
        //}

        //#endregion SQLite
    }
}
