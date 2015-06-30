using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gaea.Net.UI
{
    public partial class FormGaeaTcpServerMonitor : Form
    {
        private class MonitorObject
        {
            public string Name{set;get;}

            public string Value{set;get;}
        }

        private long preSend = 0;
        private long preRecv = 0;
        private int preTickcount = 0;



        private Hashtable datasourceMap = new Hashtable();
        private List<MonitorObject> datasource = new List<MonitorObject>();

        public FormGaeaTcpServerMonitor()
        {
            InitializeComponent();
            BuildMonitorDataSource();
            this.grdMonitor.DataSource = datasource;
            tmrMonitor.Enabled = true;
       
        }

        public Gaea.Net.Core.GaeaTcpServer GaeaTcpServer { set; get; } 

        private void tmrMonitor_Tick(object sender, EventArgs e)
        {
            MonitorObject item;
            if (GaeaTcpServer == null) return;

            DataGridView view = this.grdMonitor;
            
            item = (MonitorObject)datasourceMap["state"];
            DataGridViewCell cell = view[1, 0];
            if (GaeaTcpServer.Active)
            {                
                item.Value = "已启动";
                //cell.Value = "已启动";
            }else{
                item.Value = "未启动";
                //cell.Value = "未启动";
            }

            item = (MonitorObject)datasourceMap["online"];
            item.Value = GaeaTcpServer.Monitor.OnlineCounter.ToString();

            item = (MonitorObject)datasourceMap["accept"];
            item.Value = string.Format("创建:{0:#,0}  销毁:{1:#,0}  借出:{2:#,0}  还回:{3:#,0}  投递:{4:#,0}  响应:{5:#,0}",
                  GaeaTcpServer.Monitor.AcceptCreateCounter,
                  GaeaTcpServer.Monitor.AcceptDestroyCounter,
                  GaeaTcpServer.Monitor.AcceptRequestGetCounter,
                  GaeaTcpServer.Monitor.AcceptRequestReleaseCounter,
                  GaeaTcpServer.Monitor.AcceptPostCounter,
                  GaeaTcpServer.Monitor.AcceptResponseCounter
                    );

            item = (MonitorObject)datasourceMap["send"];
            item.Value = string.Format("字节:{0:#,0}  投递:{1:#,0}  响应:{2:#,0}  取消:{3:#,0}  剩余:{4:#,0}",
                  GaeaTcpServer.Monitor.SendSize,
                  GaeaTcpServer.Monitor.SendPostCounter,
                  GaeaTcpServer.Monitor.SendResponseCounter,
                  GaeaTcpServer.Monitor.SendCancelCounter,
                  GaeaTcpServer.Monitor.SendPostCounter - 
                    GaeaTcpServer.Monitor.SendResponseCounter - GaeaTcpServer.Monitor.SendCancelCounter
                    );

            item = (MonitorObject)datasourceMap["recv"];
            item.Value = string.Format("字节:{0:#,0}  投递:{1:#,0}  响应:{2:#,0}  剩余:{3:#,0}",
                  GaeaTcpServer.Monitor.RecvSize,
                  GaeaTcpServer.Monitor.RecvPostCounter,
                  GaeaTcpServer.Monitor.RecvResponseCounter,
                  GaeaTcpServer.Monitor.RecvPostCounter -
                    GaeaTcpServer.Monitor.RecvResponseCounter
                    );

            item = (MonitorObject)datasourceMap["objSendReq"];
            item.Value = string.Format("创建:{0:#,0}  销毁:{1:#,0}  借出:{2:#,0}  还回:{3:#,0}",
                  GaeaTcpServer.Monitor.SendRequestCreateCounter,
                  GaeaTcpServer.Monitor.SendRequestDestoryCounter,
                  GaeaTcpServer.Monitor.SendRequestGetCounter,
                  GaeaTcpServer.Monitor.SendRequestReleaseCounter
                    );


            item = (MonitorObject)datasourceMap["speed"];

            if (preTickcount != 0)
            {
                int tickcount = System.Environment.TickCount - preTickcount;
                item.Value = string.Format("接收:{0:f} kb/s 发送:{0:f} kb/s",
                    ((GaeaTcpServer.Monitor.RecvSize - preRecv) / 1024.0000) / (tickcount / 1000.0000),
                    ((GaeaTcpServer.Monitor.SendSize - preSend) / 1024.0000) / (tickcount / 1000.0000)
                    );
            }

            preRecv = GaeaTcpServer.Monitor.RecvSize;
            preSend = GaeaTcpServer.Monitor.SendSize;
            preTickcount = System.Environment.TickCount;

            item = (MonitorObject)datasourceMap["runtime"];
            item.Value = RunTime.GetRunTimeInfo();

            grdMonitor.Invalidate();
        }

        private void BuildMonitorDataSource()
        {
            MonitorObject item;

            item = new MonitorObject();
            item.Name = "服务状态";
            item.Value = "已启动";
            datasourceMap.Add("state", item);
            datasource.Add(item);

            item = new MonitorObject();
            item.Name = "在线";
            item.Value = "0";
            datasourceMap.Add("online", item);
            datasource.Add(item);

            // 
            item = new MonitorObject();
            item.Name = "连接请求";
            item.Value = "";
            datasourceMap.Add("accept", item);
            datasource.Add(item);

            // 发送信息
            item = new MonitorObject();
            item.Name = "发送信息";
            item.Value = "";
            datasourceMap.Add("send", item);
            datasource.Add(item);

            item = new MonitorObject();
            item.Name = "接收信息";
            item.Value = "";
            datasourceMap.Add("recv", item);
            datasource.Add(item);

            item = new MonitorObject();
            item.Name = "发送对象";
            item.Value = "";
            datasourceMap.Add("objSendReq", item);
            datasource.Add(item);

            item = new MonitorObject();
            item.Name = "速率";
            item.Value = "";
            datasourceMap.Add("speed", item);
            datasource.Add(item);


            item = new MonitorObject();
            item.Name = "运行时间";
            item.Value = "0 s";
            datasourceMap.Add("runtime", item);
            datasource.Add(item);

            item = new MonitorObject();
            item.Name = "启动时间";
            item.Value = string.Format("{0:yyyy-MM-dd HH:mm:ss}", DateTime.Now);
            datasourceMap.Add("startuptime", item);
            datasource.Add(item);



        }

        private void FormGaeaTcpServerMonitor_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
