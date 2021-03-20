using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace flexsys.TinyCLR.Core
{
    public abstract partial class ServiceExtensions
    {
        protected ServiceExtensions()
        {
            DebugPrintEnabled = false;
        }

        public bool DebugPrintEnabled { get; set; }

        protected void DebugPrint(object sender, string message)
        {
            Debug.WriteLine(DateTime.Now.ToString() + " : " + sender.GetType().Name + "\t : " + message);
        }

        protected void ErrorPrint(object sender, string message)
        {
            Debug.WriteLine(DateTime.Now.ToString() + " : " + sender.GetType().Name + "\t : ERROR " + message);
        }

        protected void ErrorPrint(object sender, int ErrorCode, string Message, string StackTrace)
        {
            Debug.WriteLine(
                DateTime.Now.ToString() + " : " + sender.GetType().Name + "\t : ERROR " + ErrorCode + " " + Message +
                "\r\n\t" + StackTrace);
        }
    }
}
