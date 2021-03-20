using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Text;
using flexsys.TinyCLR.Core;
using flexsys.TinyCLR.Core.Extensions;

namespace flexsys.TinyCLR.Networking.FTP.Server
{
    /// <summary>
    /// handles client commands
    /// </summary>
    internal partial class Command : ServiceExtensions
    {
        public void APPE(ref SessionObject Session, Communication com, string Argument)
        {
            try
            {
                string Path = Session.WorkingDirectory + @"\" + Argument;
                if (Session.IsDataSocketConnected)
                {
                    com.Receive(ref Session, "APPE", Path, FileMode.Append);
                }
                else
                {
                    com.SendControl(Session, "500 APPE command failed.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 APPE command failed.");
                com.Disconnect(ref Session);
            }
        }

        public void AUTH(ref SessionObject Session, Communication com, string Argument)
        {
            try
            {
                if (Session.ControlStream.GetType() == typeof(SslStream))
                {
                    com.SendControl(Session, "534 Secure connection was already negotiated.");
                    return;
                }
                switch (Argument)
                {
                    case "TLS":
                    case "TLS-C":
                        com.SendControl(Session, "234 AUTH command ok. Expecting TLS Negotiation.");

                        SslStream sslStream = new SslStream(Session.SHC.Socket);

                        DateTime timeout = DateTime.Now.AddSeconds(20);
                        while (DateTime.Now.CompareTo(timeout) <= 0)
                        {
                            try
                            {
                                sslStream.AuthenticateAsServer(Service.FTP.Configuration.Certificate, System.Security.Authentication.SslProtocols.Tls12);

                                Session.ControlStream = sslStream;

                                break;
                            }
                            catch (InvalidOperationException)
                            {
                            }
                        }
                        break;

                    case "SSL":
                        com.SendControl(Session, "504 SSL security mechanism not implemented.");
                        break;

                    case "TLS-P":
                        com.SendControl(Session, "504 TLS-P security mechanism not implemented.");
                        break;

                    default:
                        com.SendControl(Session, "501 Named security mechanism not understood.");
                        break;
                }
            }
            catch
            {
                com.SendControl(Session, "500 AUTH command failed.");
            }
        }

        public void CDUP(ref SessionObject Session, Communication com)
        {
            try
            {
                if (Session.WorkingDirectory != Service.FTP.Configuration.DocumentRoot)
                {
                    DirectoryInfo di = new DirectoryInfo(Session.WorkingDirectory);
                    Session.WorkingDirectory = di.Parent.FullName;
                }
                com.SendControl(Session, "250 Working directory changed to '" + ConvertPathToRemote(Session.WorkingDirectory) + "'.");
            }
            catch
            {
                com.SendControl(Session, "500 CDUP command failed.");
            }
        }

        public void CWD(ref SessionObject Session, Communication com, string Argument)
        {
            string newpath;
            try
            {
                if (Argument.StartsWith("/"))
                {
                    newpath = ConvertPathToLocal(Argument);
                }
                else
                {
                    newpath = Session.WorkingDirectory + "\\" + Argument;
                }

                if (Directory.Exists(newpath))
                {
                    DirectoryInfo di = new DirectoryInfo(newpath);
                    Session.WorkingDirectory = di.FullName;

                    com.SendControl(Session, "250 Working directory changed to '" + ConvertPathToRemote(Session.WorkingDirectory) + "'.");
                }
                else
                {
                    com.SendControl(Session, "550 No such directory.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 CWD command failed.");
            }
        }

        public void DELE(ref SessionObject Session, Communication com, string Argument)
        {
            string Path;
            try
            {
                Path = Session.WorkingDirectory + @"\" + Argument;
                if (File.Exists(Path))
                {
                    File.Delete(Path);
                    com.SendControl(Session, "250 File '" + Path + "' deleted successfull.\r\n");
                }
                else
                {
                    com.SendControl(Session, "550 File not found.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 DELE command failed.");
            }
        }

        public void EPSV(ref SessionObject Session, Communication com)
        {
            byte[] Port;
            try
            {
                //229 Entering Extended Passive Mode (|||54283|)
                Port = com.Bind(ref Session);
                if (Port != null)
                {
                    //string[] parts = Settings.Address.Split('.');
                    com.SendControl(Session, "227 Entering Extended Passive Mode (|||" + ToString(Port) + "|).");
                }
                else
                {
                    com.SendControl(Session, "500 EPSV command failed.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 EPSV command failed.");
            }
        }

        public void FEAT(ref SessionObject Session, Communication com)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("211-Extensions supported:");
            sb.AppendLine(" AUTH TLS;TLS-C");
            sb.AppendLine(" MDTM");
            sb.AppendLine(" MLST type*;size*;modify*;create*;");
            sb.AppendLine(" PASV");
            sb.AppendLine(" PBSZ");
            sb.AppendLine(" PROT C;P;");
            sb.AppendLine(" Rest Stream");
            sb.AppendLine(" SIZE");
            sb.Append("211 End");

            com.SendControl(Session, sb.ToString());
        }

        public void HELP(ref SessionObject Session, Communication com)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("214-The following commands are recognized.");
            sb.AppendLine(" APPE");
            sb.AppendLine(" AUTH");
            sb.AppendLine(" CDUP");
            sb.AppendLine(" CWD");
            sb.AppendLine(" CD");
            sb.AppendLine(" DELE");
            sb.AppendLine(" EPSV");
            sb.AppendLine(" FEAT");
            sb.AppendLine(" HELP");
            sb.AppendLine(" LIST");
            sb.AppendLine(" MDTM");
            sb.AppendLine(" MKD");
            sb.AppendLine(" MLSD");
            sb.AppendLine(" MLST");
            sb.AppendLine(" NLST");
            sb.AppendLine(" NOOP");
            sb.AppendLine(" PASS");
            sb.AppendLine(" PASV");
            sb.AppendLine(" PBSZ");
            sb.AppendLine(" PORT");
            sb.AppendLine(" PROT");
            sb.AppendLine(" PWD");
            sb.AppendLine(" QUIT");
            sb.AppendLine(" REST");
            sb.AppendLine(" RETR");
            sb.AppendLine(" RMD");
            sb.AppendLine(" RNFR");
            sb.AppendLine(" RNTO");
            sb.AppendLine(" SIZE");
            sb.AppendLine(" STOR");
            sb.AppendLine(" SYST");
            sb.AppendLine(" TYPE");
            sb.AppendLine(" USER");
            sb.AppendLine(" XCUP");
            sb.AppendLine(" XCWD");
            sb.AppendLine(" XMKD");
            sb.AppendLine(" XPWD");
            sb.AppendLine(" XRMD");
            sb.Append("214 HELP command successfull.");

            com.SendControl(Session, sb.ToString());
        }

        public void LIST(ref SessionObject Session, Communication com, string Argument)
        {
            string Path, TimeStamp;
            StringBuilder sb = new StringBuilder();
            try
            {
                if (Session.IsDataSocketConnected)
                {
                    if (Argument != null && Argument.Length > 0)
                    {
                        Path = Argument;
                    }
                    else
                    {
                        Path = Session.WorkingDirectory;
                    }

                    DirectoryInfo cdi = new DirectoryInfo(Path);
                    if (cdi.Exists)
                    {
                        DateTime Period = new DateTime(DateTime.Now.Ticks - TimeSpan.TicksPerDay * 180);

                        DirectoryInfo[] dirs = cdi.GetDirectories();
                        DirectoryInfo dir;
                        for (int i = 0; i < dirs.Length; i++)
                        {
                            dir = dirs[i];

                            if ((dir.Attributes & FileAttributes.System) == FileAttributes.System)
                            {
                                continue;
                            }
                            TimeStamp = dir.LastWriteTime < Period ? dir.LastWriteTime.ToString("MMM dd  yyyy") : dir.LastWriteTime.ToString("MMM dd HH:mm");
                            sb.Append("drw-rw-rw- 2 2003 2003 4096 ");
                            sb.Append(TimeStamp);
                            sb.Append(" ");
                            sb.AppendLine(dir.Name);
                        }

                        FileInfo[] files = cdi.GetFiles();
                        FileInfo file;
                        for (int i = 0; i < files.Length; i++)
                        {
                            file = files[i];

                            TimeStamp = file.LastWriteTime < Period ? file.LastWriteTime.ToString("MMM dd  yyyy") : file.LastWriteTime.ToString("MMM dd HH:mm");
                            sb.Append("-rw-rw-rw- 2 2003 2003 ");
                            sb.Append(file.Length);
                            sb.Append(" ");
                            sb.Append(TimeStamp);
                            sb.Append(" ");
                            sb.AppendLine(file.Name);
                        }
                        com.SendData(ref Session, "LIST", sb.ToString());
                    }
                    else
                    {
                        com.SendControl(Session, "550 Directory not found.");
                    }
                }
                else
                {
                    com.SendControl(Session, "500 LIST command failed.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 LIST command failed.");
                com.Disconnect(ref Session);
            }
        }

        public void MDTM(ref SessionObject Session, Communication com, string Argument)
        {
            string Path;
            try
            {
                Path = Session.WorkingDirectory + @"\" + Argument;
                if (File.Exists(Path))
                {
                    FileInfo fi = new FileInfo(Path);
                    com.SendControl(Session, "213 " + ConvertTime(fi.LastWriteTime));
                }
                else
                {
                    com.SendControl(Session, "550 File not found.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 MDTM command failed.");
            }
        }

        public void MKD(ref SessionObject Session, Communication com, string Argument)
        {
            string Path;
            try
            {
                Path = Session.WorkingDirectory + @"\" + Argument;
                Directory.CreateDirectory(Path);
                com.SendControl(Session, "250 Directory '" + ConvertPathToRemote(Path) + "' created successfull.");
            }
            catch
            {
                com.SendControl(Session, "500 MKD command failed.");
            }
        }

        public void MLSD(ref SessionObject Session, Communication com, string Argument)
        {
            StringBuilder sb = new StringBuilder();
            string pathlocal, reply;
            try
            {
                pathlocal = string.IsNullOrEmpty(Argument) ? Session.WorkingDirectory : ConvertPathToLocal(Argument);
                
                DirectoryInfo cdi = new DirectoryInfo(pathlocal);
                if (cdi.Exists)
                {
                    sb.Append("Type=cdir;Modify=");
                    sb.Append(ConvertTime(cdi.LastWriteTime));
                    sb.Append(";Create=");
                    sb.Append(ConvertTime(cdi.CreationTime));
                    sb.Append("; ");
                    sb.AppendLine(ConvertPathToRemote(pathlocal));

                    if (pathlocal != Service.FTP.Configuration.DocumentRoot)
                    {
                        sb.Append("Type=pdir;Modify=");
                        sb.Append(ConvertTime(cdi.Parent.LastWriteTime));
                        sb.Append(";Create=");
                        sb.Append(ConvertTime(cdi.Parent.CreationTime));
                        sb.Append("; ");
                        sb.AppendLine("..");
                    }

                    DirectoryInfo[] dirs = cdi.GetDirectories();
                    DirectoryInfo dir;
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        dir = dirs[i];

                        if ((dir.Attributes & FileAttributes.System) == FileAttributes.System)
                        {
                            continue;
                        }
                        sb.Append("Type=dir;Modify=");
                        sb.Append(ConvertTime(dir.LastWriteTime));
                        sb.Append(";Create=");
                        sb.Append(ConvertTime(dir.CreationTime));
                        sb.Append("; ");
                        sb.AppendLine(dir.Name);
                    }

                    FileInfo[] files = cdi.GetFiles();
                    FileInfo file;
                    for (int i = 0; i < files.Length; i++)
                    {
                        file = files[i];

                        sb.Append("Type=file;Size=");
                        sb.Append(file.Length);
                        sb.Append(";Modify=");
                        sb.Append(ConvertTime(file.LastWriteTime));
                        sb.Append(";Create=");
                        sb.Append(ConvertTime(file.CreationTime));
                        sb.Append("; ");
                        sb.AppendLine(file.Name);
                    }

                    reply = sb.ToString();

                    DebugPrint(this, "\r\n" + reply);
                    com.SendData(ref Session, "MLSD", reply);
                }
                else
                {
                    com.SendControl(Session, "550 Directory not found.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 MLSD command failed.");
                com.Disconnect(ref Session);
            }
        }

        public void MLST(ref SessionObject Session, Communication com, string Argument)
        {
            StringBuilder sb = new StringBuilder();
            string pathlocal;
            try
            {
                pathlocal = ConvertPathToLocal(Argument);

                sb.Append("250- Listing ");
                sb.AppendLine(Argument);

                if (Directory.Exists(pathlocal))
                {
                    DirectoryInfo di = new DirectoryInfo(pathlocal);
                    sb.Append(" Type=dir;Modify=");
                    sb.Append(ConvertTime(di.LastWriteTime));
                    sb.Append(";Create=");
                    sb.Append(ConvertTime(di.CreationTime));
                    sb.Append("; ");
                    sb.AppendLine(ConvertPathToRemote(di.FullName));
                }
                else if (File.Exists(pathlocal))
                {
                    FileInfo fi = new FileInfo(pathlocal);
                    sb.Append(" Type=file;Size=");
                    sb.Append(fi.Length.ToString());
                    sb.Append(";Modify=");
                    sb.Append(ConvertTime(fi.LastWriteTime));
                    sb.Append(";Create=");
                    sb.Append(ConvertTime(fi.CreationTime));
                    sb.Append("; ");
                    sb.AppendLine(ConvertPathToRemote(fi.FullName));
                }
                else
                {
                    com.SendControl(Session, "550 No such file or directory.");
                    return;
                }

                sb.Append("250 End");

                com.SendControl(Session, sb.ToString());
            }
            catch
            {
                com.SendControl(Session, "500 MLST command failed.");
            }
        }

        public void NLST(ref SessionObject Session, Communication com)
        {
            StringBuilder sb = new StringBuilder();
            string reply;
            string[] items;
            int i;
            try
            {
                if (Session.IsDataSocketConnected)
                {
                    //Reply = "\r\nContent of " + ConvertPathToRemote(Session.WorkingDirectory) + "\r\n\r\n";
                    sb.AppendLine();
                    sb.Append("Content of ");
                    sb.AppendLine(ConvertPathToRemote(Session.WorkingDirectory));
                    sb.AppendLine();

                    items = Directory.GetDirectories(Session.WorkingDirectory);
                    for (i = 0; i < items.Length; i++)
                    {
                        //Reply += " <DIR> " + items[i].Substring(Session.WorkingDirectory.Length + 1) + "\r\n";
                        sb.Append(" <DIR> ");
                        sb.AppendLine(items[i].Substring(Session.WorkingDirectory.Length + 1));
                    }
                    items = Directory.GetFiles(Session.WorkingDirectory);
                    for (i = 0; i < items.Length; i++)
                    {
                        //Reply += "       " + items[i].Substring(Session.WorkingDirectory.Length + 1) + "\r\n";
                        sb.Append("       ");
                        sb.AppendLine(items[i].Substring(Session.WorkingDirectory.Length + 1));
                    }

                    reply = sb.ToString();

                    DebugPrint(this, reply);

                    com.SendData(ref Session, "NLST", reply);
                }
                else
                {
                    com.SendControl(Session, "500 NLST command failed.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 NLST command failed.");
                com.Disconnect(ref Session);
            }
        }

        public void NOOP(ref SessionObject Session, Communication com)
        {
            com.SendControl(Session, "200 NOOP command successfull.");
        }

        public void PASS(ref SessionObject Session, Communication com, RequestObject Request)
        {
            AuthEventArgs args = new AuthEventArgs(Session.User, Request.Argument);
            if (Service.FTP.Configuration.AnonymousAuthentication)
            {
                args.IsAuthenticated = true;
            }
            else
            {
                Service.FTP.Configuration.AuthMethod(this, args);
            }
            if (args.IsAuthenticated)
            {
                com.SendControl(Session, "230 User logged in.");
                Session.WorkingDirectory = Service.FTP.Configuration.DocumentRoot;
                Session.IsAuthenticated = true;
            }
            else
            {
                Session.User = string.Empty;
                com.SendControl(Session, "530-User cannot log in.\r\n Logon failure: Unknown user name or bad password.\r\n530 End");
            }
        }

        public void PASV(ref SessionObject Session, Communication com)
        {
            try
            {
                byte[] Address = DeviceInformation.LocalAddress.GetAddressBytes();
                byte[] Port = com.Bind(ref Session);
                if (Port != null)
                {
                    string message = "227 Entering Passive Mode (" + Address[0] + "," + Address[1] + "," + Address[2] + "," + Address[3] + "," + Port[1] + "," + Port[0] + ").";
                    com.SendControl(Session, message);
                }
                else
                {
                    com.SendControl(Session, "500 PASV command failed.");
                }
            }
            catch (Exception)
            {
                com.SendControl(Session, "500 PASV command failed.");
                com.Disconnect(ref Session);
            }
        }

        public void PBSZ(ref SessionObject Session, Communication com, string Argument)
        {
            try
            {
                if (int.Parse(Argument) == 0)
                {
                    Session.PBSZ = 0;
                    com.SendControl(Session, "200 PBSZ command successfull.");
                }
                else
                {
                    com.SendControl(Session, "500 PBSZ command failed.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 PBSZ command failed.");
            }
        }

        public void PORT(ref SessionObject Session, Communication com, string Argument)
        {
            string[] addr;
            string IP;
            int Port;
            try
            {
                addr = Argument.Split(new char[] { ',' });
                IP = addr[0] + "." + addr[1] + "." + addr[2] + "." + addr[3];
                Port = (int.Parse(addr[4]) * 0x100) + int.Parse(addr[5]);

                com.Connect(ref Session, new IPEndPoint(IPAddress.Parse(IP), Port));
                Session.IsPORT = true;
                com.SendControl(Session, "200 PORT command successfull.");
            }
            catch
            {
                com.SendControl(Session, "500 PORT command failed.");
                com.Disconnect(ref Session);
            }
        }

        public void PROT(ref SessionObject Session, Communication com, string Argument)
        {
            if (Session.PBSZ == -1)
            {
                com.SendControl(Session, "503 Bad sequence of commands.");
                return;
            }
            switch (Argument)
            {
                case "C":
                    Session.PROT = "C";
                    com.SendControl(Session, "200 PROT command successful.");
                    break;

                //case "S":
                //case "E":
                case "P":
                    Session.PROT = "P";
                    com.SendControl(Session, "200 PROT command successful.");
                    break;

                default:
                    com.SendControl(Session, "504 Command not implemented for that parameter.");
                    break;
            }
        }

        public void PWD(ref SessionObject Session, Communication com)
        {
            com.SendControl(Session, "257 '" + ConvertPathToRemote(Session.WorkingDirectory) + "' is current directory.");
        }

        public void QUIT(ref SessionObject Session, Communication com)
        {
            com.SendControl(Session, "221 Goodbye.");
        }

        public void REST(ref SessionObject Session, Communication com, string Argument)
        {
            if (int.TryParse(Argument, out int marker))
            {
                Session.Marker = marker;
                com.SendControl(Session, "350 Restarting at " + Session.Marker + ". Send STORE or RETRIEVE to initiate transfer.");
            }
            else
            {
                com.SendControl(Session, "500 Syntax Error, marker no Number.");
            }
        }

        public void RETR(ref SessionObject Session, Communication com, string Argument)
        {
            string Path;
            try
            {
                Path = Session.WorkingDirectory + @"\" + Argument;
                if (Session.IsDataSocketConnected)
                {
                    if (File.Exists(Path))
                    {
                        using (DeviceResources.FileHandler FH = new DeviceResources.FileHandler(Path, FileMode.Open, FileAccess.Read))
                        {
                            if (Session.Marker != 0)
                            {
                                FH.FStream.Seek(Session.Marker, SeekOrigin.Begin);
                                Session.Marker = 0;
                            }
                            com.SendData(ref Session, "RETR", FH.FStream);
                        }
                    }
                    else
                    {
                        com.SendControl(Session, "550 File not found.");
                        com.Disconnect(ref Session);
                    }
                }
                else
                {
                    com.SendControl(Session, "500 RETR command failed.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 RETR command failed.");
                com.Disconnect(ref Session);
            }
        }

        public void RMD(ref SessionObject Session, Communication com, string Argument)
        {
            string Path;
            try
            {
                Path = Session.WorkingDirectory + @"\" + Argument;
                if (Path != Service.FTP.Configuration.DocumentRoot)
                {
                    if (Directory.Exists(Path))
                    {
                        Directory.Delete(Path);
                        com.SendControl(Session, "250 Directory '" + ConvertPathToRemote(Path) + "' deleted successfull.");
                    }
                    else
                    {
                        com.SendControl(Session, "550 Directory not found.");
                    }
                }
                else
                {
                    com.SendControl(Session, "550 The root directory refuses itself to be deleted.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 RMD command failed.");
            }
        }

        public void RNFR(ref SessionObject Session, Communication com, string Argument)
        {
            string Path = Session.WorkingDirectory + @"\" + Argument;
            try
            {
                if (File.Exists(Path) || Directory.Exists(Path))
                {
                    Session.RNFR = Path;
                    com.SendControl(Session, "350 Requested file action pending further information.");
                }
                else
                {
                    com.SendControl(Session, "550 File/Directory not found.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 RNFR command failed.");
            }
        }

        public void RNTO(ref SessionObject Session, Communication com, string Argument)
        {
            string Path = Session.WorkingDirectory + @"\" + Argument;
            try
            {
                if (Session.RNFR != string.Empty)
                {
                    if (File.Exists(Session.RNFR))
                    {
                        // File
                        if (!File.Exists(Path))
                        {
                            File.Move(Session.RNFR, Path);
                            com.SendControl(Session, "250 File renamed successfull.");
                        }
                        else
                        {
                            com.SendControl(Session, "553 File '" + Path + "' already exists.");
                        }
                    }
                    else
                    {
                        // Directory
                        if (!Directory.Exists(Path))
                        {
                            Directory.Move(Session.RNFR, Path);
                            com.SendControl(Session, "250 Directory renamed successfull.");
                        }
                        else
                        {
                            com.SendControl(Session, "553 Directory '" + Path + "' already exists.");
                        }
                    }
                }
                else
                {
                    com.SendControl(Session, "503 Bad sequence of commands.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 RNTO command failed.");
            }
            finally
            {
                Session.RNFR = string.Empty;
            }
        }

        public void SIZE(ref SessionObject Session, Communication com, string Argument)
        {
            string Path;
            try
            {
                Path = Session.WorkingDirectory + @"\" + Argument;
                if (File.Exists(Path))
                {
                    FileInfo fi = new FileInfo(Path);
                    com.SendControl(Session, "213 " + fi.Length.ToString());
                }
                else
                {
                    com.SendControl(Session, "550 File not found.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 SIZE command failed.");
            }
        }

        public void STOR(ref SessionObject Session, Communication com, string Argument)
        {
            string Path;
            try
            {
                Path = Session.WorkingDirectory + @"\" + Argument;
                if (Session.IsDataSocketConnected)
                {
                    if (Session.Marker == 0)
                    {
                        com.Receive(ref Session, "STOR", Path, FileMode.Create);
                    }
                    else
                    {
                        com.Receive(ref Session, "STOR", Path, FileMode.Open);
                    }
                }
                else
                {
                    com.SendControl(Session, "500 STOR command failed.");
                }
            }
            catch
            {
                com.SendControl(Session, "500 STOR command failed.");
                com.Disconnect(ref Session);
            }
        }

        public void SYST(ref SessionObject Session, Communication com)
        {
            com.SendControl(Session, "215 CloudServices .NET Micro Framework");
        }

        public void TYPE(ref SessionObject Session, Communication com, string Argument)
        {
            if (Argument == "A" || Argument == "I")
            {
                com.SendControl(Session, "215 TYPE set to " + Argument + ".");
            }
            else
            {
                com.SendControl(Session, "501 Parameter not understood.");
            }
        }

        public void USER(ref SessionObject Session, Communication com, string Argument)
        {
            Session.User = Argument;
            if (Session.User == "anonymous" && Service.FTP.Configuration.AnonymousAuthentication)
            {
                com.SendControl(Session, "331 Anonymous access allowed, send identity (e-mail name) as password.");
            }
            else
            {
                com.SendControl(Session, "331 Password required for " + Session.User + ".");
            }
        }
    }
}