using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaea.Net.Core
{
    public class SocketServer
    {
        Hashtable onlineMap = new Hashtable();

        /// <summary>
        ///  添加一个连接到在线列表中
        /// </summary>
        /// <param name="context"></param>
        public void AddContext(SocketContext context)
        {
            context.OwnerServer = this;
            lock (onlineMap)
            {
                onlineMap.Add(context.RawSocket.Handle, context);
            }
        }

        /// <summary>
        ///  移除一个在线连接
        /// </summary>
        /// <param name="context"></param>
        public void RemoveContext(SocketContext context)
        {
            lock (onlineMap)
            {
                onlineMap.Remove(context.RawSocket.Handle);
            }
        }

        /// <summary>
        ///  请求断开所有连接, 不一定会立刻断开
        /// </summary>
        public void RequestDisconnectAll()
        {
            lock(onlineMap)
            {
                foreach(SocketContext context in onlineMap)
                {
                    context.RequestDisconnect();
                }
            }
        }
    }
}
