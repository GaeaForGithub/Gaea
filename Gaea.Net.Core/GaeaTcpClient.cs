using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Gaea.Net.Core
{
    public class SocketConnectRequest : GaeaSocketRequest
    {
        public GaeaTcpClientContext Context { set; get; }

        public override void DoResponse()
        {
            base.DoResponse();
            Context.DoAfterConnectRequest(this);
        }
    }


    public class GaeaTcpClientContext:GaeaSocketContext
    {
        private SocketConnectRequest connectRequest = new SocketConnectRequest();

        public GaeaTcpClientContext():base()
        {
            connectRequest.Context = this;
        }
        

        /// <summary>
        ///  异步请求连接完成后执行的
        /// </summary>
        /// <param name="req"></param>
        public void DoAfterConnectRequest(SocketConnectRequest req)
        {
            if (req.SocketEventArg.SocketError == SocketError.Success)
            {
                DoAfterConnected();
                this.PostReceiveRequest();
            }
            else
            {
                this.LogMessage(string.Format(GaeaStrRes.STR_ConnectRequestException, 
                    SocketHandle, req.SocketEventArg.SocketError), LogLevel.lgvDebug);
                if (OwnerServer!=null)
                {
                    ((GaeaTcpClient)OwnerServer).DoConnectAsyncError(req);
                }
                RawSocket.Close();
                RawSocket = null;
            }
        }

        /// <summary>
        ///   发起阻塞的连接请求
        /// </summary>
        public void Connect()
        {
            if (RawSocket != null)
            {
                throw new Exception(GaeaStrRes.STR_ConnectionIsCreated);
            }

            RawSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAddr = GaeaNetUtils.ExtractFirstIPV4Address(Host);
            IPEndPoint p = new IPEndPoint(ipAddr, Port);

            RawSocket.Connect(p);

            DoAfterConnected();
            this.PostReceiveRequest();        
        }

        /// <summary>
        ///  发起异步连接请求
        /// </summary>
        public void ConnectAsync()
        {
            if (RawSocket != null)
            {
                throw new Exception(GaeaStrRes.STR_ConnectionIsCreated);
            }

            RawSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAddr = GaeaNetUtils.ExtractFirstIPV4Address(Host);
            IPEndPoint p = new IPEndPoint(ipAddr, Port);
            connectRequest.SocketEventArg.RemoteEndPoint = p;
            


            RawSocket.ConnectAsync(connectRequest.SocketEventArg);
        }

        public string Host { set; get; }

        public int Port { set; get; }
    }

    public class GaeaTcpClient:GaeaSocketBase
    {
        private List<GaeaTcpClientContext> clientContexts = new List<GaeaTcpClientContext>();

        public event OnContextErrorEvent OnContextConnectAsyncError;

        public void DoConnectAsyncError(SocketConnectRequest req)
        {
            if (OnContextConnectAsyncError !=null)
            {
                OnContextConnectAsyncError(req.Context, req.SocketEventArg.SocketError);
            }
        }

        public void BindClientContext(GaeaTcpClientContext context)
        {
            context.OwnerServer = this;
            lock (clientContexts)
            {
                clientContexts.Add(context);                
            }
        }

        public void UnBindClientContext(GaeaTcpClientContext context)
        {
            context.OwnerServer = null;
            lock (clientContexts)
            {
                clientContexts.Remove(context);
            }
        }
    }
}
