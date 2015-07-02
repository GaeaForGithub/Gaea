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
   
    public static class SocketContextPoolCenter
    {
        public static GaeaObjectPool<SocketSendRequest> SendRequestPool = new GaeaObjectPool<SocketSendRequest>();
    }


    public class SocketSendRequest:GaeaSocketRequest
    {
        public GaeaObjectPool<SocketSendRequest> Pool { set; get; }

        public GaeaMonitor Monitor { set; get; }

        private ManualResetEvent blockEvent = new ManualResetEvent(true);

        /// <summary>
        ///  阻塞Event，响应时会设置信号
        /// </summary>
        public ManualResetEvent BlockEvent { get { return blockEvent; } }

        ~SocketSendRequest()
        {
            if (Monitor != null)
            {
                Monitor.IncSendRequestDestroyCounter();
            }         
        }

        public void DoCleanUp()
        {
            IsCloseRequest = false;
            blockEvent.Reset();
        }

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

#if TRACE_DETAIL
                Context.LogMessage(String.Format(GaeaStrRes.STR_TRACE_SendRequestResponse,
                    Context.SocketHandle, SocketEventArg.BytesTransferred), LogLevel.lgvTrace);
#endif                
                
                blockEvent.Set();

                Context.SendNextRequest();
            }
            else
            {
                blockEvent.Set();

                Context.LogMessage(String.Format(GaeaStrRes.STR_SendContextException,
                    Context.SocketHandle, SocketEventArg.BytesTransferred, SocketEventArg.SocketError), LogLevel.lgvDebug);

                Context.RequestDisconnect();
            }

            if (Pool != null)
            {
                Pool.ReleaseObject(this);
                Pool = null;
                if (Monitor != null)
                {
                    Monitor.IncSendRequestReleaseCounter();
                }
            }
            else
            {
                this.SocketEventArg.Dispose();
            }
  
        }

        /// <summary>
        ///  请求被取消
        ///  一般情况下是，连接关闭时，发送队列还没有来得及处理
        /// </summary>
        public virtual void DoCancel()
        {
            Context.IncSendCancelCounter();

            if (Pool != null)
            {
                Pool.ReleaseObject(this);
                Pool = null;
                if (Monitor!=null)
                {
                    Monitor.IncSendRequestReleaseCounter();
                }
            }
            else
            {
                this.SocketEventArg.Dispose();
            }
        }

        /// <summary>
        ///  取消请求，暂时没找到好的方法，该方法暂时闲职
        ///   设计思想:
        ///     投的的请求，底层还未来得及处理时，进行取消。
        /// </summary>
        public void CancelRequest()
        {
        }
        
    }


    /// <summary>
    ///   接受请求类    
    /// </summary>    
    public class SocketReceiveRequest:GaeaSocketRequest
    {
        private ManualResetEvent blockEvent = new ManualResetEvent(true);

        /// <summary>
        ///  阻塞Event，投递时可以手动重置信号，响应时会自动设置信号
        /// </summary>
        public ManualResetEvent BlockEvent { get { return blockEvent; } }

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

                blockEvent.Set();
            }
            else
            {
                blockEvent.Set();

                Context.LogMessage(String.Format(GaeaStrRes.STR_ReceiveException,
                    Context.SocketHandle, SocketEventArg.BytesTransferred, SocketEventArg.SocketError), LogLevel.lgvDebug);

                Context.RequestDisconnect();
            }

        }
    }

    

    /// <summary>
    ///  连接对应的上下文
    ///  阻塞接收暂时未处理.暂时没有好的思路。
    ///   （如果阻塞接收的话，不能在一开始就投递接收请求）
    /// </summary>
    public class GaeaSocketContext
    {
        public GaeaObjectPool<GaeaSocketContext> Pool { set; get; }

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

        public void IncSendRequestCreateCounter()
        {
            if (OwnerServer != null)
            {
                OwnerServer.Monitor.IncSendRequestCreateCounter();
            }
        }

        public void IncSendRequestDestroyCounter()
        {
            if (OwnerServer != null)
            {
                OwnerServer.Monitor.IncSendRequestDestroyCounter();
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
        ///  执行清理工作，归还到对象池时进行清理
        /// </summary>
        public virtual void DoCleanUp()
        {

        }


        /// <summary>
        ///  记录日志
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="level"></param>
        public void LogMessage(string msg, LogLevel level)
        {
            if (OwnerServer != null)
            {
                OwnerServer.LogMessage(msg, level);
            }
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
            if (OwnerServer != null)
            {
                // 移除在线连接
                OwnerServer.RemoveFromOnlineList(this);
                OwnerServer.DoContextDisconnected(this);
                OwnerServer.Monitor.DecOnline();
            }
            this.ClearSendCache();
            RawSocket.Close();
            RawSocket = null;
            active = false;
            // 执行清理工作
            DoCleanUp();
            Pool.ReleaseObject(this);
        }

        /// <summary>
        ///  接受到连接后，调用
        /// </summary>
        public virtual void DoAfterConnected()
        {
            SocketHandle = RawSocket.Handle;
            requestedDisconnect = false;
            sending = false;
            AddRef();
            if (OwnerServer != null)
            {
                OwnerServer.DoContextConnected(this);
                OwnerServer.Monitor.IncOnline();
            }
            active = true;
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
            SocketSendRequest req = GetSocketSendRequest();
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
#if TRACE_DETAIL
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

        public GaeaSocketBase OwnerServer { set; get; }

        public void DoRecveiveBuffer(SocketAsyncEventArgs e)
        {
            if (this.AddRef())
            {
                try
                {
                    OnRecvBuffer(e);

                    if (OwnerServer != null)
                    {
                        OwnerServer.DoContextReceive(this, e);
                    }

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

        /// <summary>
        ///  获取一个SocketSendRequest对象
        /// </summary>
        /// <returns></returns>
        private SocketSendRequest GetSocketSendRequest()
        {
            SocketSendRequest req = SocketContextPoolCenter.SendRequestPool.GetObject();
            if (req == null)
            {
                req = new SocketSendRequest();
                this.IncSendRequestCreateCounter();
            }

            if (OwnerServer != null)
            {
                OwnerServer.Monitor.IncSendRequestGetCounter();
            }

            req.DoCleanUp();

            if (OwnerServer != null)
            {
                req.Monitor = OwnerServer.Monitor;
            }
            req.Pool = SocketContextPoolCenter.SendRequestPool; 
            req.Context = this;
            return req;
        }
    

        /// <summary>
        ///  阻塞方式发送一段buffer, 阻塞至成功发送或者，请求被取消(连接断开)
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <param name="len"></param>
        /// <param name="copyBuffer"></param>
        /// <returns>返回成功发送的字节数</returns>
        public int BlockSendBuffer(byte[] buffer, int startIndex, int len, bool copyBuffer)
        {
            SocketSendRequest req = GetSocketSendRequest();
            
            if (copyBuffer)
            {
                byte[] tmpBuffer = new byte[len];
                Array.Copy(buffer, startIndex, tmpBuffer, 0, len);
                req.SocketEventArg.SetBuffer(tmpBuffer, 0, len);
            }
            else
            {
                req.SocketEventArg.SetBuffer(buffer, startIndex, len);
            }
            req.BlockEvent.Reset();
            PostSendRequest(req);
            req.BlockEvent.WaitOne();            
            return req.SocketEventArg.BytesTransferred;
        }

        /// <summary>
        ///  阻塞发送一个字符串, 阻塞至成功发送或者，请求被取消(连接断开)
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="strEncoding"></param>
        /// <returns>返回成功发送的字节数</returns>
        public int BlockSendString(string msg, Encoding strEncoding)
        {
            byte[] buf = strEncoding.GetBytes(msg);
            int rvalue = BlockSendBuffer(buf, 0, buf.Length, false);
            return rvalue;
        }

        public void PostSendRequest(SocketSendRequest req)
        {
            bool start = false;
            lock(sendCache)
            {                
                sendCache.Add(req);                
                if (!sending)
                {
                    sending = true;
                    start = true;
                }
            }

            if (OwnerServer != null)
            {
                OwnerServer.Monitor.IncSendPostCounter();
            }
    
            if (start)
            {
                SendNextRequest();
            }
        }

        /// <summary>
        ///  投递一个发送请求
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="startIndex"></param>
        /// <param name="len"></param>
        /// <param name="copyBuffer"></param>
        public void PostSendRequest(byte[] buffer, int startIndex, int len, bool copyBuffer)
        {
            SocketSendRequest req = GetSocketSendRequest();

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
            recvRequest.BlockEvent.Reset();

            if (!RawSocket.ReceiveAsync(recvRequest.SocketEventArg))
            {
                recvRequest.DoResponse();
            }

            if (OwnerServer != null)
            {
                OwnerServer.Monitor.IncRecvPostCounter();
            }
            
        }

        protected bool active = false;

        public bool Active { get { return active; } }
    }
}
