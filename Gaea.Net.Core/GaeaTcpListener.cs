using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace Gaea.Net.Core
{


    public class AcceptRequest : GaeaSocketRequest
    {
        public override void DoResponse()
        {
            base.DoResponse();
            Owner.IncAcceptResponse();

            bool allowAccept = true;
            Owner.TcpServer.DoAccept(SocketEventArg.AcceptSocket, ref allowAccept);

            if (allowAccept)
            {
                if (SocketEventArg.SocketError == SocketError.Success)
                {
                    Owner.DoAfterAccept(this);
                }
                else
                {
                    Owner.TcpServer.LogMessage(String.Format(GaeaStrRes.STR_AcceptException,
                        Owner.TcpServer.Name, SocketEventArg.SocketError), LogLevel.lgvDebug);
                    Owner.CheckPostRequest();
                }
            }
           
        }
        public GaeaTcpListener Owner { set; get; }
    }


    public class GaeaTcpListener
    {
        private Socket socket = null;

        Type socketContextClassType = null;


        public void IncAcceptResponse()
        {
            TcpServer.Monitor.IncAcceptResponseCounter();
        }
        

        public GaeaSocketContext GetSocketContext()
        {
            GaeaSocketContext context = (GaeaSocketContext)System.Activator.CreateInstance(socketContextClassType);            
            return context;
        }

        public void RegisterContextClass(Type classType)
        {
            socketContextClassType = classType;
        }


        private AcceptRequest GetAcceptRequest()
        {
            AcceptRequest req = new AcceptRequest();
            req.Owner = this;
            return req;
        }

        private void ReleaseAcceptRequest(AcceptRequest req)
        {
            req.Owner = null;
            return;
        }  


        public void CheckPostRequest()
        {
            AcceptRequest req = GetAcceptRequest();
            if (!PostAcceptRequest(req))
            {   // 投递失败
                ReleaseAcceptRequest(req);
            }
        }


        public void DoAfterAccept(AcceptRequest req)
        {
            GaeaSocketContext context = GetSocketContext();
            context.RawSocket = req.SocketEventArg.AcceptSocket;
            context.OwnerServer = TcpServer;
            TcpServer.AddToOnlineList(context);
            context.DoAfterConnected();
            context.PostReceiveRequest();

            // 投递另外的接收请求
            CheckPostRequest();
        }
        

        /// <summary>
        ///   开始侦听
        /// </summary>
        /// <param name="localEndPoint"></param>
        public void Start(IPEndPoint localEndPoint)
        {
            socket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
            socket.Listen(0);

            Debug.WriteLine(String.Format("服务已经开启,侦听端口:{0}", localEndPoint.Port));
        }

        /// <summary>
        ///   开启侦听端口
        /// </summary>
        /// <param name="port"></param>
        public void Start(int port)
        {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, port);
            Start(ip);
        }

        /// <summary>
        ///  开启侦听端口(Port属性端口)
        /// </summary>
        public void Start()
        {
            IPAddress ipAddr;
            if (string.IsNullOrEmpty(Host))
            {
                ipAddr = IPAddress.Any;
            }else
            {           
                ipAddr = IPAddress.Parse(Host);
            }
             
            IPEndPoint p = new IPEndPoint(ipAddr, Port);
            Start(p);
        }

        public void Stop()
        {
            lock (this)
            {
                if (socket != null)
                {
                    socket.Close();
                    socket = null;
                }
            }
        }

        /// <summary>
        ///  投递一个接收请求
        /// </summary>
        /// <param name="request"></param>
        private bool PostAcceptRequest(GaeaSocketRequest request)
        {
            bool iodepending = true;
            lock (this)
            {
                if (socket == null) return false;
                request.SocketEventArg.AcceptSocket = null;
                iodepending = socket.AcceptAsync(request.SocketEventArg);                
                TcpServer.Monitor.IncAcceptPostCounter();
            }
            if (!iodepending)
            {   // returns false if the I/O operation completed synchronously
                request.DoResponse();
            }
            return true;
        }

        /// <summary>
        ///  绑定的TcpServer
        /// </summary>
        public GaeaTcpServer TcpServer { set; get; }


        /// <summary>
        ///  设定侦听端口, 可以用Start()进行开启
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        ///  设定绑定IP, 默认为空，绑定所有的IP("0.0.0.0")
        /// </summary>
        public string Host { set; get; }

    }
}
