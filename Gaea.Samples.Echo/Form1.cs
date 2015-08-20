using Gaea.Net.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gaea.Samples.Echo
{
    public partial class Form1 : Form
    {
        private GaeaTcpServer tcpSvr = new GaeaTcpServer();
        public Form1()
        {
            InitializeComponent();
            tcpSvr.Name = "ECHO服务";
            tcpSvr.OnContextRecvBuffer += OnRecvBuffer;
            tcpSvr.OnContextConnected += OnContextConnected;
            tcpSvr.OnContextDisconnected += OnContextDisconnected;
            Gaea.Net.UI.GaeaUIFactory.CreateGaeaTcpServerMonitor(pnlMonitor, tcpSvr);
                
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        public void OnContextConnected(GaeaSocketContext context)
        {
            // 如果输出日志，会占用主线程资源
            Debug.WriteLine(String.Format("[{0}:{1}]建立连接成功!",
                 context.RemoteHost, context.RemotePort));
        }

        public void OnContextDisconnected(GaeaSocketContext context)
        {
            // 如果输出日志，会占用主线程资源
            Debug.WriteLine(String.Format("[{0}:{1}]断开连接!",
                 context.RemoteHost, context.RemotePort));
        }

        public void OnRecvBuffer(GaeaSocketContext context, byte[] buffer, int len)
        {
            // 如果输出日志，会占用主线程资源
            //Debug.WriteLine(String.Format("{0}接收到数据, 长度:{1}", context.RawSocket.Handle, len));

            // 投递回客户端
            context.PostSendRequest(buffer, 0, len, true);

            // 避免处理太快
            // Thread.Sleep(0);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (tcpSvr.Active)
            {
                tcpSvr.Stop();
                btnStart.Text = "开启服务";
            }
            else
            {
                tcpSvr.DefaultPort = int.Parse(txtPort.Text);

                // 注册自己的Context扩展类
                //tcpSvr.DefaultListener.RegisterContextClass(typeof(GaeaSocketContext));

                tcpSvr.Open();
                btnStart.Text = "停止服务";
            }
            

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            tcpSvr.Stop();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string s = "ab中国";
            byte[] buffer = ASCIIEncoding.Default.GetBytes(s);

            string s2 = ASCIIEncoding.Default.GetString(buffer);

            IPAddress[] lst = Dns.GetHostAddresses("www.baidu.com");  // 也可以用机器名
            
            foreach (IPAddress ip in lst)
            {
                MessageBox.Show(ip.ToString());
            }            
        }

        void DoTask()
        {
            IList<GaeaSocketContext> list = new List<GaeaSocketContext>();
            tcpSvr.GetOnlineList(list);
            while (tcpSvr.Active)
            {
                foreach (GaeaSocketContext context in list)
                {
                    if (context.AddRef("Test"))
                    {
                        try
                        {
                            context.PostSendString("test push!");
                            Thread.Sleep(1000);

                        }
                        finally
                        {
                           // Debug.WriteLine(String.Format("[{0}:{1}]task_context_release!", context.RemoteHost, context.RemotePort));
                            context.ReleaseRef("Test");
                        }

                    }
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Task task = new Task(DoTask);
            task.Start();
        }
    }
}
