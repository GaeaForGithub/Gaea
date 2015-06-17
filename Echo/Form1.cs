using Gaea.Net.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Echo
{
    public partial class Form1 : Form
    {
        private GaeaTcpServer tcpSvr = new GaeaTcpServer();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {


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
                tcpSvr.DefaultListener.RegisterContextClass(typeof(GaeaSocketContext));
                tcpSvr.Open();
                btnStart.Text = "停止服务";
            }
            

        }
    }
}
