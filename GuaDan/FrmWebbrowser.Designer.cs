namespace GuaDan
{
    partial class FrmWebbrowser
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
            this.toolForward = new System.Windows.Forms.ToolStripButton();
            this.toolBack = new System.Windows.Forms.ToolStripButton();
            this.toolRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolClear = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).BeginInit();
            this.SuspendLayout();
            // 
            // toolForward
            // 
            this.toolForward.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolForward.Name = "toolForward";
            this.toolForward.Size = new System.Drawing.Size(43, 28);
            this.toolForward.Text = "前进";
            this.toolForward.Click += new System.EventHandler(this.toolForward_Click);
            // 
            // toolBack
            // 
            this.toolBack.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBack.Name = "toolBack";
            this.toolBack.Size = new System.Drawing.Size(43, 28);
            this.toolBack.Text = "后退";
            this.toolBack.Click += new System.EventHandler(this.toolBack_Click);
            // 
            // toolRefresh
            // 
            this.toolRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolRefresh.Name = "toolRefresh";
            this.toolRefresh.Size = new System.Drawing.Size(43, 28);
            this.toolRefresh.Text = "刷新";
            this.toolRefresh.Click += new System.EventHandler(this.toolRefresh_Click);
            // 
            // toolClear
            // 
            this.toolClear.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolClear.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolClear.Name = "toolClear";
            this.toolClear.Size = new System.Drawing.Size(43, 28);
            this.toolClear.Text = "清除";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolForward,
            this.toolBack,
            this.toolRefresh,
            this.toolClear});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Margin = new System.Windows.Forms.Padding(0, 25, 0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0);
            this.toolStrip1.Size = new System.Drawing.Size(1662, 31);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // webView21
            // 
            this.webView21.AllowExternalDrop = true;
            this.webView21.CreationProperties = null;
            this.webView21.DefaultBackgroundColor = System.Drawing.Color.White;
            this.webView21.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webView21.Location = new System.Drawing.Point(0, 31);
            this.webView21.Margin = new System.Windows.Forms.Padding(4);
            this.webView21.Name = "webView21";
            this.webView21.Size = new System.Drawing.Size(1662, 1002);
            this.webView21.TabIndex = 4;
            this.webView21.ZoomFactor = 1D;
            this.webView21.NavigationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs>(this.webView21_NavigationCompleted);
            // 
            // FrmWebbrowser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1662, 1033);
            this.Controls.Add(this.webView21);
            this.Controls.Add(this.toolStrip1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MinimumSize = new System.Drawing.Size(1394, 1028);
            this.Name = "FrmWebbrowser";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FrmWebbrowser";
            this.Load += new System.EventHandler(this.FrmWeb_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.webView21)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStripButton toolForward;
        private System.Windows.Forms.ToolStripButton toolBack;
        private System.Windows.Forms.ToolStripButton toolRefresh;
        private System.Windows.Forms.ToolStripButton toolClear;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
    }
}