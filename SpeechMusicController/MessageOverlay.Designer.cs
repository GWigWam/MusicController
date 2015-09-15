namespace SpeechMusicController {
    partial class MessageOverlay {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageOverlay));
            this.T_AutoClose = new System.Windows.Forms.Timer(this.components);
            this.Lb_Message = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // T_AutoClose
            // 
            this.T_AutoClose.Interval = 1000;
            this.T_AutoClose.Tick += new System.EventHandler(this.T_AutoClose_Tick);
            // 
            // Lb_Message
            // 
            this.Lb_Message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Lb_Message.Font = new System.Drawing.Font("Trebuchet MS", 56F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Lb_Message.Location = new System.Drawing.Point(0, 0);
            this.Lb_Message.Name = "Lb_Message";
            this.Lb_Message.Size = new System.Drawing.Size(1920, 100);
            this.Lb_Message.TabIndex = 0;
            this.Lb_Message.Text = "Lorum Ipsem dolar sit amet";
            this.Lb_Message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MessageOverlay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1920, 100);
            this.Controls.Add(this.Lb_Message);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MessageOverlay";
            this.Opacity = 0.4D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "MessageOverlay";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.MessageOverlay_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer T_AutoClose;
        private System.Windows.Forms.Label Lb_Message;
    }
}