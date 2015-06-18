﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaea.Net.Core
{
    public class GaeaTcpServer:SocketServer
    {
        List<TcpListener> listeners = new List<TcpListener>();
        TcpListener defaultListener = null;
        private bool active;

        public GaeaTcpServer():base()
        {
            defaultListener = new TcpListener();
            defaultListener.TcpServer = this;
            defaultListener.RegisterContextClass(typeof(SocketContext));
        }

        public void Open()
        {
            defaultListener.Start();
            defaultListener.CheckPostRequest();      
            active = true;
        }

        public void Stop()
        {
            if (active)
            {
                active = false;
                // 停止监听
                defaultListener.Stop();

                // 请求断开
                RequestDisconnectAll();

                // 等待连接关闭
                WaitForContextRelease();

                LogMessage(String.Format(StrRes.STR_ServerOff, Name), LogLevel.lgvDebug);
            }
        }

        public int DefaultPort { 
            set
            {
                defaultListener.Port = value;
            }
            get
            {
                return defaultListener.Port;
            }
        }

        public TcpListener DefaultListener { get { return defaultListener; } }

        public bool Active { get { return active; } set { } }


    }
}
