namespace SpeechMusicController {
    partial class ListeningTimer {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
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
            this.NotifyText = new System.Windows.Forms.Label();
            this.CountdownTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // NotifyText
            // 
            this.NotifyText.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.NotifyText.AutoSize = true;
            this.NotifyText.Font = new System.Drawing.Font("Microsoft Sans Serif", 60F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NotifyText.ForeColor = System.Drawing.SystemColors.ActiveCaption;
            this.NotifyText.Location = new System.Drawing.Point(0, 0);
            this.NotifyText.Margin = new System.Windows.Forms.Padding(0);
            this.NotifyText.Name = "NotifyText";
            this.NotifyText.Size = new System.Drawing.Size(88, 91);
            this.NotifyText.TabIndex = 0;
            this.NotifyText.Text = "T";
            this.NotifyText.TextChanged += new System.EventHandler(this.NotifyText_TextChanged);
            // 
            // CountdownTimer
            // 
            this.CountdownTimer.Enabled = true;
            this.CountdownTimer.Interval = 500;
            this.CountdownTimer.Tick += new System.EventHandler(this.CountdownTimer_Tick);
            // 
            // ListeningTimer
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(100, 100);
            this.Controls.Add(this.NotifyText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(100, 100);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(100, 100);
            this.Name = "ListeningTimer";
            this.Opacity = 0.5D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "NotifyForm";
            this.TopMost = true;
            this.MouseEnter += new System.EventHandler(this.NotifyForm_MouseEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label NotifyText;
        protected System.Windows.Forms.Timer CountdownTimer;
    }
}