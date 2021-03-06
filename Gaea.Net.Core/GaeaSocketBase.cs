﻿using System;
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

    public delegate void OnContextErrorEvent(GaeaSocketContext context, SocketError error);        

    public delegate void OnAcceptEvent(Socket acceptSocket, ref bool allowAccept);

    public class GaeaSocketBase
    {
        Hashtable onlineMap = new Hashtable();
        ManualResetEvent releaseEvent = new ManualResetEvent(true);
        GaeaMonitor monitor = new GaeaMonitor();

        public GaeaMonitor Monitor { get { return monitor; } }

        /// <summary>
        ///  添加一个连接到在线列表中
        /// </summary>
        /// <param name="context"></param>
        public void AddToOnlineList(GaeaSocketContext context)
        {
            lock (onlineMap)
            {
                onlineMap.Add(context.RawSocket.Handle, context);
                releaseEvent.Reset();
            }
        }

        /// <summary>
        ///  从在线列表中查找对应的连接
        /// </summary>
        /// <param name="socketHandle"></param>
        /// <returns></returns>
        public GaeaSocketContext FindContextInOnlineList(IntPtr socketHandle)
        {
            object rvalue = onlineMap[socketHandle];
            return (GaeaSocketContext)rvalue;
        }

        public int GetOnlineList(IList<GaeaSocketContext> list)
        {
            int rvalue = 0;
            lock(onlineMap)
            {
                foreach (DictionaryEntry obj in onlineMap)
                {
                    list.Add((GaeaSocketContext)obj.Value); 
                    rvalue++;
                }
            }
        
            return rvalue;
        }

        /// <summary>
        ///  移除一个在线连接
        /// </summary>
        /// <param name="context"></param>
        public void RemoveFromOnlineList(GaeaSocketContext context)
        {
            lock (onlineMap)
            {
                if (context.RawSocket == null)
                {
#if DEBUG
                    throw new Exception("UnException, Assert in RemoveFromOnlineList");
#else
                    onlineMap.Remove(context.SocketHandle);
#endif
                }
                else
                {
                    onlineMap.Remove(context.RawSocket.Handle);
                } 
                if (onlineMap.Count == 0)
                {
                    releaseEvent.Set();
                }
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
            releaseEvent.WaitOne();
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
                context.RequestDisconnect("RequestDisconnectAll");
            }
        }

        public void LogMessage(string msg, LogLevel level)
        {
            Debug.WriteLine(msg);
        }

        public string Name { set; get; }
    }
}
