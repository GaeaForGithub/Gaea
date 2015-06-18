﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gaea.Net.Core
{
    public class SocketSendRequest:SocketRequest
    {
        public SocketContext Context { set; get; }
        public override void DoResponse()
        {
            base.DoResponse();

            if (SocketEventArg.BytesTransferred > 0 && SocketEventArg.SocketError == SocketError.Success)
            {
                Context.SendNextRequest();
            }
            else
            {
                Context.LogMessage(String.Format(StrRes.STR_SendContextException,
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

        }
        
    }



    public class SocketReceiveRequest:SocketRequest
    {
        public SocketContext Context { set; get; }
        public override void DoResponse()
        {
            base.DoResponse();

            if (SocketEventArg.BytesTransferred > 0 && SocketEventArg.SocketError == SocketError.Success)
            {
                // 触发接收事件
                Context.DoRecveiveBuffer(this.SocketEventArg);
            }
            else
            {
                Context.LogMessage(String.Format(StrRes.STR_ReceiveException,
                    Context.RawSocket.Handle, SocketEventArg.BytesTransferred, SocketEventArg.SocketError), LogLevel.lgvDebug);

                Context.RequestDisconnect();
            }

        }
    }

    

    public class SocketContext
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

        /// <summary>
        ///  清理发送缓存，关闭时进行清理
        /// </summary>
        public void ClearSendCache()
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


        public void LogMessage(string msg, LogLevel level)
        {
            OwnerServer.LogMessage(msg, level);
        }

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

        private void CloseContext()
        {
            // 移除在线连接
            OwnerServer.RemoveContext(this);
            RawSocket.Close();
        }

        public void DoAfterAccept()
        {
            SocketHandle = RawSocket.Handle;
            requestedDisconnect = false;
            AddRef();           
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

        public SocketContext()
        {
                   
        }

        /// <summary>
        ///  请求断开连接，如果连接正在工作(refcounter > 0)，会等待连接停止工作时再进行断开操作
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

                    // 处理一个异步发送请求
                    if (!RawSocket.SendAsync(req.SocketEventArg))
                    {
                        req.DoResponse();
                    }
                }
                finally
                {
                    this.ReleaseRef();
                }
            }
            else
            {
                LogMessage(String.Format(StrRes.STR_SendContextIsOff,
                    SocketHandle, sendCache.Count), LogLevel.lgvDebug);
            }
        }

        public Socket RawSocket { set; get; }

        public SocketServer OwnerServer { set; get; }

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
                LogMessage(String.Format(StrRes.STR_ReceiveContextIsOff,
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
        }
    }
}
