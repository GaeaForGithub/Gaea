using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Gaea.Net.Core
{
    public static class GaeaNetUtils
    {
        /// <summary>
        ///   获取第一个IPAddress, 如果没有返回null
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IPAddress ExtractFirstIPAddress(string host)
        {
            IPAddress[] lst = Dns.GetHostAddresses(host);  // 机器名,域名

            if (lst.Length == 0) return null;

            return lst[0];
        }



        /// <summary>
        ///   获取第一个IPV4的IP地址
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static IPAddress ExtractFirstIPV4Address(string host)
        {
            IPAddress[] lst = Dns.GetHostAddresses(host);  // 机器名,域名

            foreach (IPAddress ip in lst)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }

            return null;
        }
    }
}
