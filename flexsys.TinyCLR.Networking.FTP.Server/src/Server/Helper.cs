using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace flexsys.TinyCLR.Networking.FTP.Server
{
    internal partial class Command
    {
        protected string ConvertPathToLocal(string Directory)
        {
            if (string.IsNullOrEmpty(Directory))
            {
                return Service.FTP.Configuration.DocumentRoot;
            }

            StringBuilder sb = new StringBuilder();

            sb.Append(Service.FTP.Configuration.DocumentRoot);
            
            char[] chars = Directory.ToCharArray();
            char c;
            for (int i = 0; i < chars.Length; i++)
            {
                c = chars[i];

                if (c == '/')
                {
                    sb.Append('\\');
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        protected string ConvertPathToRemote(string Directory)
        {
            StringBuilder sb = new StringBuilder();
           
            sb.Append('/');
            
            char[] chars = Directory.ToCharArray(3, Directory.Length - 3);  // without leading A|B:\
            char c;
            for (int i = 0; i < chars.Length; i++)
            {
                c = chars[i];

                if (c == '\\')
                {
                    sb.Append('/');
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        protected string ConvertTime(DateTime pDateTime)
        {
            string year = pDateTime.Year.ToString();

            string month = pDateTime.Month.ToString();
            if (month.Length == 1)
            {
                month = "0" + month;
            }

            string day = pDateTime.Day.ToString();
            if (day.Length == 1)
            {
                day = "0" + day;
            }

            string hour = pDateTime.Hour.ToString();
            if (hour.Length == 1)
            {
                hour = "0" + hour;
            }

            string minute = pDateTime.Minute.ToString();
            if (minute.Length == 1)
            {
                minute = "0" + minute;
            }

            string second = pDateTime.Second.ToString();
            if (second.Length == 1)
            {
                second = "0" + second;
            }

            return year + month + day + hour + minute + second;
        }

        protected string GetPath(string URI)
        {
            string Path = Service.FTP.Configuration.DocumentRoot;
            string[] segments = URI.Split(new char[] { '/' });
            for (int i = 0; i < segments.Length; i++)
            {
                Path = Path + segments[i] + ((i < (segments.Length - 1)) ? @"\" : "");
            }

            return File.Exists(Path) ? Path : null;
        }

        protected string ToString(byte[] value)
        {
            try
            {
                char[] chars = Encoding.UTF8.GetChars(value);
                return new string(chars);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
