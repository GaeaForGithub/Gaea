using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gaea.Net.UI
{
    public class GaeaTextEditSafeLogger
    {
        private System.Windows.Forms.TextBox txtLog;


        public GaeaTextEditSafeLogger(TextBox txtLogOut)
        {
            this.txtLog = txtLogOut;
            Enable = true;
        }

        public bool Enable { set; get; }

        public void LogMessage(string s)
        {
            if (!Enable) return;

            try
            {
                if (txtLog == null) return;
                txtLog.Invoke(
                   (MethodInvoker)delegate()
                   {
                       this.txtLog.Text += s + "\r\n";
                       txtLog.Select(txtLog.Text.Length, 0);
                       txtLog.ScrollToCaret();
                   }
               );
            }catch
            {

            }
        }
    }
}
