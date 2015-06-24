using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gaea.Net.Core
{
    public class GaeaTcpServer:GaeaSocketBase
    {
        List<GaeaTcpListener> listeners = new List<GaeaTcpListener>();
        GaeaTcpListener defaultListener = null;
        private bool active;

        public GaeaTcpServer():base()
        {
            defaultListener = new GaeaTcpListener();
            defaultListener.TcpServer = this;
            defaultListener.RegisterContextClass(typeof(GaeaSocketContext));
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

                LogMessage(String.Format(GaeaStrRes.STR_ServerOff, Name), LogLevel.lgvDebug);
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

        public GaeaTcpListener DefaultListener { get { return defaultListener; } }

        public bool Active { get { return active; } set { } }


    }
}
