using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

namespace flexsys.TinyCLR.Core
{
    public sealed partial class DeviceResources
    {
        private const int MAX_OPEN_FILEHANDLES = 8;
        private static int _OpenFileHandles = 0;

        public sealed class FileHandler : ServiceExtensions, IDisposable
        {
            public FileStream FStream { get; set; }

            public FileHandler()
            {
                while (_OpenFileHandles == MAX_OPEN_FILEHANDLES)
                {
                    Thread.Sleep(_Random.Next(100));
                }
                _OpenFileHandles++;
                Print();
            }

            public FileHandler(string path, FileMode mode, FileAccess access, FileShare share = FileShare.None)
            {
                DebugPrint(this, string.Empty);
                while (_OpenFileHandles == MAX_OPEN_FILEHANDLES)
                {
                    Thread.Sleep(_Random.Next(100));
                }
                _OpenFileHandles++;
                FStream = new FileStream(path, mode, access, share);
                Print();
            }

            private void Print()
            {
                DebugPrint(this, "\r\n\tOPEN_FILEHANDLES      : " + _OpenFileHandles);
            }

            public void Dispose()
            {
                if (FStream != null)
                {
                    FStream.Close();
                    FStream.Dispose();
                    FStream = null;
                }
                _OpenFileHandles--;
                Print();
            }
        }
    }
}