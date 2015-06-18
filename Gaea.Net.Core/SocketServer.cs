using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gaea.Net.Core
{
    public enum LogLevel {lgvMessage, lgvDebug, lgvWarning, lgvError};

    /// <summary>
    ///   接收数据事件
    /// </summary>
    /// <param name="context">发生接收的连接</param>
    /// <param name="buffer">数据流</param>
    /// <param name="len">接收到的数据长度</param>
    public delegate void OnContextRecvBufferEvent(SocketContext context, byte[] buffer, int len);

    public delegate void OnContextEvent(SocketContext context);

    public delegate void OnAcceptEvent(Socket acceptSocket, ref bool allowAccept);

    public class SocketServer
    {
        Hashtable onlineMap = new Hashtable();
        ManualResetEvent realseEvent = new ManualResetEvent(true);

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
                realseEvent.Reset();
            }
        }

        /// <summary>
        ///  连接事件
        /// </summary>
        public event OnContextEvent OnContextConnected;

        /// <summary>
        ///  断开事件
        /// </summary>
        public event OnContextEvent OnContextDisconnected;

        /// <summary>
        ///   接收到数据
        /// </summary>
        public event OnContextRecvBufferEvent OnContextRecvBuffer;

        /// <summary>
        ///  请求连接事件
        /// </summary>
        public event OnAcceptEvent OnAccept;


        /// <summary>
        ///  连接接收到数据时调用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="e"></param>
        public void DoContextReceive(SocketContext context, SocketAsyncEventArgs e)
        {
            if (OnContextRecvBuffer!=null)
            {
                OnContextRecvBuffer(context, e.Buffer, e.BytesTransferred);
            }
        }



        public void WaitForContextRelease()
        {
            LogMessage(String.Format(StrRes.STR_WaitContextRelease, Name), LogLevel.lgvDebug);
            realseEvent.WaitOne();
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
                if (onlineMap.Count== 0)
                {
                    realseEvent.Set();
                }
            }
        }

        /// <summary>
        ///  请求断开所有连接, 不一定会立刻断开
        /// </summary>
        public void RequestDisconnectAll()
        {
            List<SocketContext> lst = new List<SocketContext>();
            lock(onlineMap)
            {                
                foreach(DictionaryEntry obj in onlineMap)
                {
                    lst.Add((SocketContext)obj.Value);                    
                }                
            }

            foreach (SocketContext context in lst)
            {
                context.RequestDisconnect();
            }
        }

        public void LogMessage(string msg, LogLevel level)
        {
            Debug.WriteLine(msg);
        }

        public string Name { set; get; }
    }
}
