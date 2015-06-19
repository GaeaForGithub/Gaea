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
    public enum LogLevel {lgvTrace, lgvDebug, lgvMessage, lgvWarning, lgvError };

    /// <summary>
    ///   接收数据事件
    /// </summary>
    /// <param name="context">发生接收的连接</param>
    /// <param name="buffer">数据流</param>
    /// <param name="len">接收到的数据长度</param>
    public delegate void OnContextRecvBufferEvent(GaeaSocketContext context, byte[] buffer, int len);

    public delegate void OnContextEvent(GaeaSocketContext context);

    public delegate void OnAcceptEvent(Socket acceptSocket, ref bool allowAccept);

    public class GaeaSocketServer
    {
        Hashtable onlineMap = new Hashtable();
        ManualResetEvent realseEvent = new ManualResetEvent(true);
        GaeaMonitor monitor = new GaeaMonitor();

        public GaeaMonitor Monitor { get { return monitor; } }

        /// <summary>
        ///  添加一个连接到在线列表中
        /// </summary>
        /// <param name="context"></param>
        public void AddContext(GaeaSocketContext context)
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

        public void DoContextConnected(GaeaSocketContext context)
        {
            if (OnContextConnected != null)
            {
                OnContextConnected(context);
            }
        }



        public void DoContextDisconnected(GaeaSocketContext context)
        {
            if (OnContextDisconnected != null)
            {
                OnContextDisconnected(context);
            }
        }


        public void DoAccept(Socket acceptSocket, ref bool allowAccept)
        {
            if (OnAccept != null)
            {
                OnAccept(acceptSocket, ref allowAccept);
            }
        }


        /// <summary>
        ///  连接接收到数据时调用
        /// </summary>
        /// <param name="context"></param>
        /// <param name="e"></param>
        public void DoContextReceive(GaeaSocketContext context, SocketAsyncEventArgs e)
        {
            if (OnContextRecvBuffer!=null)
            {
                OnContextRecvBuffer(context, e.Buffer, e.BytesTransferred);
            }
        }



        public void WaitForContextRelease()
        {
            LogMessage(String.Format(GaeaStrRes.STR_WaitContextRelease, Name), LogLevel.lgvDebug);
            realseEvent.WaitOne();
        }

        /// <summary>
        ///  移除一个在线连接
        /// </summary>
        /// <param name="context"></param>
        public void RemoveContext(GaeaSocketContext context)
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
            List<GaeaSocketContext> lst = new List<GaeaSocketContext>();
            lock(onlineMap)
            {                
                foreach(DictionaryEntry obj in onlineMap)
                {
                    lst.Add((GaeaSocketContext)obj.Value);                    
                }                
            }

            foreach (GaeaSocketContext context in lst)
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
