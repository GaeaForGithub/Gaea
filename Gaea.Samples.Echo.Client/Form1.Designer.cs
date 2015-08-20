namespace Gaea.Samples.Echo.Client
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.pnlTop = new System.Windows.Forms.Panel();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtSend = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.pnlRecv = new System.Windows.Forms.Panel();
            this.txtRecv = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlTop.SuspendLayout();
            this.pnlRecv.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.Controls.Add(this.btnClose);
            this.pnlTop.Controls.Add(this.btnSend);
            this.pnlTop.Controls.Add(this.txtSend);
            this.pnlTop.Controls.Add(this.txtPort);
            this.pnlTop.Controls.Add(this.txtHost);
            this.pnlTop.Controls.Add(this.btnConnect);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(859, 170);
            this.pnlTop.TabIndex = 3;
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(619, 52);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 7;
            this.btnSend.Text = "发送";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtSend
            // 
            this.txtSend.Location = new System.Drawing.Point(3, 52);
            this.txtSend.Multiline = true;
            this.txtSend.Name = "txtSend";
            this.txtSend.Size = new System.Drawing.Size(594, 99);
            this.txtSend.TabIndex = 6;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(151, 12);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(66, 21);
            this.txtPort.TabIndex = 5;
            this.txtPort.Text = "9983";
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(12, 12);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(109, 21);
            this.txtHost.TabIndex = 4;
            this.txtHost.Text = "127.0.0.1";
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(223, 10);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 3;
            this.btnConnect.Text = "连接";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // pnlRecv
            // 
            this.pnlRecv.Controls.Add(this.txtRecv);
            this.pnlRecv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRecv.Location = new System.Drawing.Point(0, 170);
            this.pnlRecv.Name = "pnlRecv";
            this.pnlRecv.Size = new System.Drawing.Size(859, 318);
            this.pnlRecv.TabIndex = 4;
            // 
            // txtRecv
            // 
            this.txtRecv.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtRecv.Location = new System.Drawing.Point(0, 0);
            this.txtRecv.Multiline = true;
            this.txtRecv.Name = "txtRecv";
            this.txtRecv.Size = new System.Drawing.Size(859, 318);
            this.txtRecv.TabIndex = 0;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(314, 10);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 8;
            this.btnClose.Text = "断开";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(859, 488);
            this.Controls.Add(this.pnlRecv);
            this.Controls.Add(this.pnlTop);
            this.Name = "Form1";
            this.Text = "Gaea.Net.Echo.Client";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.pnlRecv.ResumeLayout(false);
            this.pnlRecv.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Panel pnlRecv;
        private System.Windows.Forms.TextBox txtRecv;
        private System.Windows.Forms.TextBox txtSend;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnClose;
    }
}

