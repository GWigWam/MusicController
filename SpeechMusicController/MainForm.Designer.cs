namespace SpeechMusicController {
    partial class MainForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.Tb_Output = new System.Windows.Forms.TextBox();
            this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItemRefresh = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemShow = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItemExit = new System.Windows.Forms.ToolStripMenuItem();
            this.KeyInput = new System.Windows.Forms.TextBox();
            this.Bt_Rules = new System.Windows.Forms.Button();
            this.Bt_Refresh = new System.Windows.Forms.Button();
            this.Bt_Settings = new System.Windows.Forms.Button();
            this.Bt_SpeechSwitch = new System.Windows.Forms.Button();
            this.contextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // Tb_Output
            // 
            this.Tb_Output.AcceptsReturn = true;
            this.Tb_Output.Cursor = System.Windows.Forms.Cursors.Default;
            this.Tb_Output.Location = new System.Drawing.Point(12, 38);
            this.Tb_Output.Multiline = true;
            this.Tb_Output.Name = "Tb_Output";
            this.Tb_Output.ReadOnly = true;
            this.Tb_Output.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Tb_Output.Size = new System.Drawing.Size(179, 84);
            this.Tb_Output.TabIndex = 0;
            // 
            // NotifyIcon
            // 
            this.NotifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.NotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("NotifyIcon.Icon")));
            this.NotifyIcon.Visible = true;
            this.NotifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(this.NotifyIcon_MouseClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemRefresh,
            this.MenuItemShow,
            this.toolStripSeparator1,
            this.MenuItemExit});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(114, 76);
            // 
            // MenuItemRefresh
            // 
            this.MenuItemRefresh.Name = "MenuItemRefresh";
            this.MenuItemRefresh.Size = new System.Drawing.Size(113, 22);
            this.MenuItemRefresh.Text = "Refresh";
            this.MenuItemRefresh.Click += new System.EventHandler(this.MenuItemRefresh_Click);
            // 
            // MenuItemShow
            // 
            this.MenuItemShow.Name = "MenuItemShow";
            this.MenuItemShow.Size = new System.Drawing.Size(113, 22);
            this.MenuItemShow.Text = "Show";
            this.MenuItemShow.Click += new System.EventHandler(this.MenuItemShow_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(110, 6);
            // 
            // MenuItemExit
            // 
            this.MenuItemExit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.MenuItemExit.Name = "MenuItemExit";
            this.MenuItemExit.Size = new System.Drawing.Size(113, 22);
            this.MenuItemExit.Text = "Exit";
            this.MenuItemExit.Click += new System.EventHandler(this.MenuItemExit_Click);
            // 
            // KeyInput
            // 
            this.KeyInput.Location = new System.Drawing.Point(12, 12);
            this.KeyInput.Name = "KeyInput";
            this.KeyInput.Size = new System.Drawing.Size(178, 20);
            this.KeyInput.TabIndex = 2;
            this.KeyInput.KeyUp += new System.Windows.Forms.KeyEventHandler(this.KeyInput_KeyUp);
            // 
            // Bt_Rules
            // 
            this.Bt_Rules.Location = new System.Drawing.Point(197, 41);
            this.Bt_Rules.Name = "Bt_Rules";
            this.Bt_Rules.Size = new System.Drawing.Size(75, 23);
            this.Bt_Rules.TabIndex = 3;
            this.Bt_Rules.Text = "Rules";
            this.Bt_Rules.UseVisualStyleBackColor = true;
            this.Bt_Rules.Click += new System.EventHandler(this.Bt_Rules_Click);
            // 
            // Bt_Refresh
            // 
            this.Bt_Refresh.Location = new System.Drawing.Point(197, 12);
            this.Bt_Refresh.Name = "Bt_Refresh";
            this.Bt_Refresh.Size = new System.Drawing.Size(75, 23);
            this.Bt_Refresh.TabIndex = 4;
            this.Bt_Refresh.Text = "Refresh";
            this.Bt_Refresh.UseVisualStyleBackColor = true;
            this.Bt_Refresh.Click += new System.EventHandler(this.Bt_Refresh_Click);
            // 
            // Bt_Settings
            // 
            this.Bt_Settings.Location = new System.Drawing.Point(197, 70);
            this.Bt_Settings.Name = "Bt_Settings";
            this.Bt_Settings.Size = new System.Drawing.Size(75, 23);
            this.Bt_Settings.TabIndex = 5;
            this.Bt_Settings.Text = "Settings";
            this.Bt_Settings.UseVisualStyleBackColor = true;
            this.Bt_Settings.Click += new System.EventHandler(this.Bt_Settings_Click);
            // 
            // Bt_SpeechSwitch
            // 
            this.Bt_SpeechSwitch.Location = new System.Drawing.Point(197, 99);
            this.Bt_SpeechSwitch.Name = "Bt_SpeechSwitch";
            this.Bt_SpeechSwitch.Size = new System.Drawing.Size(75, 23);
            this.Bt_SpeechSwitch.TabIndex = 6;
            this.Bt_SpeechSwitch.Text = "Speech (On)";
            this.Bt_SpeechSwitch.UseVisualStyleBackColor = true;
            this.Bt_SpeechSwitch.Click += new System.EventHandler(this.Bt_SpeechSwitch_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.ClientSize = new System.Drawing.Size(284, 130);
            this.Controls.Add(this.Bt_SpeechSwitch);
            this.Controls.Add(this.Bt_Settings);
            this.Controls.Add(this.Bt_Refresh);
            this.Controls.Add(this.Bt_Rules);
            this.Controls.Add(this.KeyInput);
            this.Controls.Add(this.Tb_Output);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.ShowInTaskbar = false;
            this.Text = "SpeechMusicController";
            this.TopMost = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Deactivate += new System.EventHandler(this.Form1_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.frmMain_Resize);
            this.contextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Tb_Output;
        private System.Windows.Forms.NotifyIcon NotifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem MenuItemExit;
        private System.Windows.Forms.TextBox KeyInput;
        private System.Windows.Forms.ToolStripMenuItem MenuItemShow;
        private System.Windows.Forms.Button Bt_Rules;
        private System.Windows.Forms.Button Bt_Refresh;
        private System.Windows.Forms.ToolStripMenuItem MenuItemRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.Button Bt_Settings;
        private System.Windows.Forms.Button Bt_SpeechSwitch;
    }
}

