using Gaea.Net.Core;
using Gaea.Net.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gaea.Samples.Echo.Client
{
    public partial class Form1 : Form
    {
        private GaeaTcpClient tcpClient = new GaeaTcpClient();
        private GaeaTcpClientContext clientContext = new GaeaTcpClientContext();
        public GaeaTextEditSafeLogger sflogger = null;


        public Form1()
        {
            InitializeComponent();
            sflogger = new GaeaTextEditSafeLogger(this.txtRecv);
            sflogger.Enable = true;

            tcpClient.BindClientContext(clientContext);
            tcpClient.OnContextRecvBuffer += tcpClient_OnContextRecvBuffer;
            tcpClient.OnContextConnectAsyncError += tcpClient_OnContextConnectAsyncError;
            tcpClient.OnContextConnected += tcpClient_OnContextConnected;
            tcpClient.OnContextDisconnected += tcpClient_OnContextDisconnected;
        }

        void tcpClient_OnContextDisconnected(GaeaSocketContext context)
        {
            sflogger.LogMessage("连接与服务器断开!");
        }

        void tcpClient_OnContextConnected(GaeaSocketContext context)
        {
            sflogger.LogMessage("建立连接成功!");
        }


        void tcpClient_OnContextConnectAsyncError(GaeaSocketContext context, System.Net.Sockets.SocketError error)
        {
            sflogger.LogMessage("建立连接异常:" + error.ToString());
        }

        void tcpClient_OnContextRecvBuffer(GaeaSocketContext context, byte[] buffer, int len)
        {
            string recv = Encoding.Default.GetString(buffer, 0, len);
            sflogger.LogMessage(recv);

        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (clientContext.Active)
            {
                sflogger.LogMessage("已经建立连接...");
                return;
            }

            clientContext.Host = txtHost.Text;
            clientContext.Port = int.Parse(txtPort.Text);
            clientContext.ConnectAsync();

        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            clientContext.PostSendString(txtSend.Text, Encoding.Default);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
