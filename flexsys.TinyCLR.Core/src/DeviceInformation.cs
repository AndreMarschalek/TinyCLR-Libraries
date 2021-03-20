using System;
using System.Collections;
using System.Net;
using System.Text;
using System.Threading;

namespace flexsys.TinyCLR.Core
{
    public class DeviceInformation
    {
        public static IPAddress LocalAddress
        {
            get
            {
                IPAddress[] iPAddresses = Dns.GetHostEntry("").AddressList;
                IPAddress iPAddress;
                for (int i = 0; i < iPAddresses.Length; i++)
                {
                    iPAddress = iPAddresses[i];

                    if (null == iPAddress)
                    {
                        continue;
                    }

                    byte[] addr = iPAddress.GetAddressBytes();
                    if (addr == null || addr.Length != 4)
                    {
                        continue;
                    }
                    
                    if ((addr[0] == 127) && (addr[1] == 0) && (addr[2] == 0) && (addr[3] == 1))
                    {
                        continue;
                    }

                    return iPAddress;
                }
                return null;
            }
        }
    }
}
