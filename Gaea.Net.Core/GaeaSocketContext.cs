using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gaea.Net.Core
{
    public class SocketSendRequest:GaeaSocketRequest
    {

        public GaeaSocketContext Context { set; get; }

        /// <summary>
        ///  关闭请求标志，处理该请求时会请求关闭连接(requestDisconnect)
        /// </summary>
        public bool IsCloseRequest { set; get; }

        public override void DoResponse()
        {
            base.DoResponse();
            Context.IncSendResponseCounter();

            if (SocketEventArg.BytesTransferred > 0 && SocketEventArg.SocketError == SocketError.Success)
            {
                Context.IncSendSize(SocketEventArg.BytesTransferred);

#if DEBUG
                Context.LogMessage(String.Format(GaeaStrRes.STR_TRACE_SendRequestResponse,
                    Context.SocketHandle, SocketEventArg.BytesTransferred), LogLevel.lgvTrace);
#endif                
                Context.SendNextRequest();
            }
            else
            {
                Context.LogMessage(String.Format(GaeaStrRes.STR_SendContextException,
                    Context.SocketHandle, SocketEventArg.BytesTransferred, SocketEventArg.SocketError), LogLevel.lgvDebug);

                Context.RequestDisconnect();
            }

            
        }

        /// <summary>
        ///  请求被取消
        ///  一般情况下是，连接关闭时，发送队列还没有来得及处理
        /// </summary>
        public virtual void DoCancel()
        {
            Context.IncSendCancelCounter();
        }
        
    }



    public class SocketReceiveRequest:GaeaSocketRequest
    {
        public GaeaSocketContext Context { set; get; }
        public override void DoResponse()
        {
            base.DoResponse();
            Context.IncRecvResponseCounter();
            if (SocketEventArg.BytesTransferred > 0 && SocketEventArg.SocketError == SocketError.Success)
            {
                Context.IncRecvSize(SocketEventArg.BytesTransferred);

                // 触发接收事件
                Context.DoRecveiveBuffer(this.SocketEventArg);
            }
            else
            {
                Context.LogMessage(String.Format(GaeaStrRes.STR_ReceiveException,
                    Context.RawSocket.Handle, SocketEventArg.BytesTransferred, SocketEventArg.SocketError), LogLevel.lgvDebug);

                Context.RequestDisconnect();
            }

        }
    }

    

    public class GaeaSocketContext
    {
        private int refcount = 0;

        // 是否已经请求了断开操作
        private bool requestedDisconnect = false;

        private bool sending = false;
        private byte[] recvBuffer = null;
        private int recvBufferLen = 1024 * 50;
        private SocketReceiveRequest recvRequest = null;
        public List<SocketSendRequest> sendCache = new List<SocketSendRequest>();
        public IntPtr SocketHandle { get; set; }

        public void IncSendResponseCounter()
        {
            if (OwnerServer != null)
            {
                OwnerServer.Monitor.IncSendResponseCounter();
            }
        }

        public void IncSendSize(long size)
        {
            if (OwnerServer != null)
            {
                OwnerServer.Monitor.IncSendSize(size);
            }
        }

        public void IncSendCancelCounter()
        {
            if (OwnerServer != null)
            {
                OwnerServer.Monitor.IncSendCancelCounter();
            }
        }


        public void IncRecvPostCounter()
        {
            if (OwnerServer != null)
            {
                OwnerServer.Monitor.IncRecvPostCounter();
            }
        }

        public void IncRecvResponseCounter()
        {
            if (OwnerServer != null)
            {
                OwnerServer.Monitor.IncRecvResponseCounter();
            }
        }

        public void IncRecvSize(long size)
        {
            if (OwnerServer != null)
            {
                OwnerServer.Monitor.IncRecvSize(size);
            }
        }



        /// <summary>
        ///  清理发送缓存，关闭时进行清理
        /// </summary>
        private void ClearSendCache()
        {
            lock (sendCache)
            {
                foreach (SocketSendRequest req in sendCache)
                {
                    req.DoCancel();
                }
                sendCache.Clear();
            }
        }


        /// <summary>
        ///  记录日志
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="level"></param>
        public void LogMessage(string msg, LogLevel level)
        {
            OwnerServer.LogMessage(msg, level);
        }

        /// <summary>
        ///  检测对象是否创建
        /// </summary>
        private void CheckInitalizeObjects()
        {
            if (recvRequest == null)
            {
                recvBuffer = new byte[recvBufferLen];
                recvRequest = new SocketReceiveRequest();
                recvRequest.Context = this;
                recvRequest.SocketEventArg.SetBuffer(recvBuffer, 0, recvBufferLen);
            }
               
        }

        /// <summary>
        ///  关闭连接，由ReleaseRef调用
        /// </summary>
        private void CloseContext()
        {
            // 移除在线连接
            OwnerServer.RemoveContext(this);
            OwnerServer.DoContextDisconnected(this);
            OwnerServer.Monitor.DecOnline();
            this.ClearSendCache();
            RawSocket.Close();
        }

        /// <summary>
        ///  接受到连接后，调用
        /// </summary>
        public void DoAfterAccept()
        {
            SocketHandle = RawSocket.Handle;
            requestedDisconnect = false;
            AddRef();
            OwnerServer.DoContextConnected(this);
            OwnerServer.Monitor.IncOnline();
        }

        /// <summary>
        ///  增加引用计数, 如果成功，代表可以使用
        /// </summary>
        public bool AddRef()
        {
            lock (this)
            {
                if (requestedDisconnect) return false;
                refcount++;
                return true;
            }
        }

        /// <summary>
        ///  减少引用计数器，0时进行释放
        /// </summary>
        public void ReleaseRef()
        {
            int j = 0;
            lock (this)
            {                
                refcount--;;
                j = refcount;
            }

            if (j==0)
            {
                // 断开连接
                CloseContext();
            }
        }

        public GaeaSocketContext()
        {
                   
        }

        /// <summary>
        ///  请求断开连接，如果连接正在工作(refcounter > 0)，会等待连接停止工作时再进行断开操作
        ///  执行后，会清未完成的发送请求，会取消Socket动作, 而且也不会再处理新请求。
        /// </summary>
        public void RequestDisconnect()
        {
            bool release = false;
            lock(this)
            {
                if (!requestedDisconnect)
                {
                    RawSocket.Shutdown(SocketShutdown.Both);                    
                    requestedDisconnect = true;
                    release = true;
                }
            }

            if (release)
            {   // 是否创建的时候添加的引用
                ReleaseRef();
            }
        }

        /// <summary>
        ///   投递一个关闭请求，到发送缓存队列，会等待前面的发送请求完成后，进行请求关闭动作。
        ///   如果该连接请求了关闭，将不会再处理该请求。
        /// </summary>
        public void PostDisconnectRequest()
        {
            SocketSendRequest req = new SocketSendRequest();
            req.IsCloseRequest = true;
            PostSendRequest(req);
        }



        /// <summary>
        ///  投递一个字符串
        /// </summary>
        /// <param name="msg">要发送的字符串</param>
        /// <param name="strEncoding">字符串编码, 可以使用Encoding中获取编码方式</param>
        /// <returns>返回投递的长度</returns>
        public int PostSendString(string msg, Encoding strEncoding)
        {
            byte[] buf = strEncoding.GetBytes(msg);
            PostSendRequest(buf, 0, buf.Length, false);
            return buf.Length;
        }

        /// <summary>
        ///  投递一个字符串，用系统Default编码进行编码
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>返回投递的长度</returns>
        public int PostSendString(string msg)
        {            
            return PostSendString(msg, Encoding.Default);
        }

        /// <summary>
        ///  从发送缓存队列中取出数据，然后进行发送
        /// </summary>
        public void SendNextRequest()
        {
            if (this.AddRef())
            {
                try
                {
                    SocketSendRequest req = null;
                    lock (sendCache)
                    {
                        if (sendCache.Count == 0)
                        {
                            sending = false;
                            return;
                        }
                        req = sendCache[0];
                        sendCache.RemoveAt(0);
                    }

                    if (req.IsCloseRequest)
                    {   // 关闭请求
#if DEBUG
                        LogMessage(String.Format(GaeaStrRes.STR_PostDisconnectRequest,
                            SocketHandle), LogLevel.lgvTrace);
#endif      

                        // 请求关闭当前连接
                        this.RequestDisconnect();
                    }
                    else
                    {
                        // 处理一个异步发送请求
                        if (!RawSocket.SendAsync(req.SocketEventArg))
                        {
                            req.DoResponse();
                        }
                    }
                }
                finally
                {
                    this.ReleaseRef();
                }
            }
            else
            {
                LogMessage(String.Format(GaeaStrRes.STR_SendContextIsOff,
                    SocketHandle, sendCache.Count), LogLevel.lgvDebug);
            }
        }

        public Socket RawSocket { set; get; }

        public string RemoteHost { 
            get
            {
                return ((System.Net.IPEndPoint)RawSocket.RemoteEndPoint).Address.ToString();
            }
        }

        public int RemotePort
        {
            get
            {
                return ((System.Net.IPEndPoint)RawSocket.RemoteEndPoint).Port;
            }
        }

        public GaeaSocketServer OwnerServer { set; get; }

        public void DoRecveiveBuffer(SocketAsyncEventArgs e)
        {
            if (this.AddRef())
            {
                try
                {
                    OnRecvBuffer(e);

                    OwnerServer.DoContextReceive(this, e);

                    // 继续投递一个接收请求
                    PostReceiveRequest();
                }
                finally
                {
                    this.ReleaseRef();
                }
            }else
            {
                LogMessage(String.Format(GaeaStrRes.STR_ReceiveContextIsOff,
                    SocketHandle), LogLevel.lgvDebug);
            }
        }

        public virtual void OnRecvBuffer(SocketAsyncEventArgs e)
        {
            
        }

        public void PostSendRequest(SocketSendRequest req)
        {
            bool start = false;
            req.Context = this;
            lock(sendCache)
            {                
                sendCache.Add(req);
                OwnerServer.Monitor.IncSendPostCounter();
                if (!sending)
                {
                    sending = true;
                    start = true;
                }
            }
    
            if (start)
            {
                SendNextRequest();
            }
        }

        public void PostSendRequest(byte[] buffer, int startIndex, int len, bool copyBuffer)
        {
            SocketSendRequest req = new SocketSendRequest();
            if (copyBuffer)
            {
                byte[] tmpBuffer = new byte[len];
                Array.Copy(buffer, startIndex, tmpBuffer, 0, len);
                req.SocketEventArg.SetBuffer(tmpBuffer, 0, len);
            } else
            {
                req.SocketEventArg.SetBuffer(buffer, startIndex, len);
            }            
            PostSendRequest(req);
        }

        public void PostReceiveRequest()
        {
            CheckInitalizeObjects();
            
            if (!RawSocket.ReceiveAsync(recvRequest.SocketEventArg))
            {
                recvRequest.DoResponse();
            }
            OwnerServer.Monitor.IncRecvPostCounter();
            
        }
    }
}
