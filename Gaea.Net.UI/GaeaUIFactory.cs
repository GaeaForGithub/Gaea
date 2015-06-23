using Gaea.Net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gaea.Net.UI
{
    public static class GaeaUIFactory
    {
        public static void CreateGaeaTcpServerMonitor(Control parent, GaeaTcpServer monitorTcpServer)
        {
            FormGaeaTcpServerMonitor form = new FormGaeaTcpServerMonitor();
            form.GaeaTcpServer = monitorTcpServer;
            form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            form.TopLevel = false;
            form.Parent = parent;
            //form.Visible = true;
            form.Show();
            form.Dock = DockStyle.Fill;
        }
    }
}
