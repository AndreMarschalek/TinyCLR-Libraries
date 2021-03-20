using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace flexsys.TinyCLR.Networking.FTP.Server
{
    public class Configuration
    {
        private const string IDENTIFIER = "TinyCLR OS Cloud Services/1.0";

        public Configuration()
        {
        }

        public Method Method { get; set; }

        public bool AnonymousAuthentication { get; set; }

        public AuthCallback AuthMethod { get; set; }

        public X509Certificate Certificate { get; set; }

        public string WelcomeMessage { get; set; } = IDENTIFIER;

        /// <summary>
        /// The Directory out of which you will serve your items.
        /// </summary>
        public string DocumentRoot { get; set; }

        public int ServerPort { get; set; }

        /// <summary>
        /// Socket Listen
        /// </summary>
        public int Backlog { get; set; } = 10;

        /// <summary>
        /// minutes
        /// </summary>
        public int SessionTimeout { get; set; } = 10;

        /// <summary>
        /// minutes
        /// </summary>
        public int IpsBlockTime { get; set; } = 3;

        public int MaxConnectionsPerMinute { get; set; } = 3;

        public int MaxWrongPasswords { get; set; } = 3;
    }
}
