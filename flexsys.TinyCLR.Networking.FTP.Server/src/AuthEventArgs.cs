using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace flexsys.TinyCLR.Networking.FTP.Server
{
    public delegate void AuthCallback(object sender, AuthEventArgs e);

    public class AuthEventArgs
    {
        public bool IsAuthenticated = false;
        public string Username = string.Empty;
        public string Password = string.Empty;

        public AuthEventArgs(string Username, string Password)
        {
            this.Username = Username;
            this.Password = Password;
        }
    }
}
