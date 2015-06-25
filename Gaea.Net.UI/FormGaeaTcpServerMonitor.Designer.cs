namespace Gaea.Net.UI
{
    partial class FormGaeaTcpServerMonitor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tmrMonitor = new System.Windows.Forms.Timer(this.components);
            this.grdMonitor = new System.Windows.Forms.DataGridView();
            this.colItemID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.grdMonitor)).BeginInit();
            this.SuspendLayout();
            // 
            // tmrMonitor
            // 
            this.tmrMonitor.Interval = 1000;
            this.tmrMonitor.Tick += new System.EventHandler(this.tmrMonitor_Tick);
            // 
            // grdMonitor
            // 
            this.grdMonitor.AllowUserToAddRows = false;
            this.grdMonitor.AllowUserToDeleteRows = false;
            this.grdMonitor.AllowUserToResizeColumns = false;
            this.grdMonitor.AllowUserToResizeRows = false;
            this.grdMonitor.BackgroundColor = System.Drawing.SystemColors.Window;
            this.grdMonitor.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.grdMonitor.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.grdMonitor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdMonitor.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colItemID,
            this.colValue});
            this.grdMonitor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdMonitor.Location = new System.Drawing.Point(0, 0);
            this.grdMonitor.Name = "grdMonitor";
            this.grdMonitor.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.grdMonitor.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.grdMonitor.RowHeadersVisible = false;
            this.grdMonitor.RowTemplate.Height = 23;
            this.grdMonitor.Size = new System.Drawing.Size(712, 397);
            this.grdMonitor.TabIndex = 1;
            this.grdMonitor.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // colItemID
            // 
            this.colItemID.DataPropertyName = "Name";
            this.colItemID.HeaderText = "项目";
            this.colItemID.Name = "colItemID";
            this.colItemID.ReadOnly = true;
            this.colItemID.Width = 150;
            // 
            // colValue
            // 
            this.colValue.DataPropertyName = "Value";
            this.colValue.HeaderText = "项目值";
            this.colValue.Name = "colValue";
            this.colValue.ReadOnly = true;
            this.colValue.Width = 500;
            // 
            // FormGaeaTcpServerMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(712, 397);
            this.Controls.Add(this.grdMonitor);
            this.Name = "FormGaeaTcpServerMonitor";
            this.Text = "FormGaeaTcpServerMonitor";
            this.Load += new System.EventHandler(this.FormGaeaTcpServerMonitor_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grdMonitor)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer tmrMonitor;
        private System.Windows.Forms.DataGridView grdMonitor;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValue;
    }
}