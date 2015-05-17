namespace SpeechMusicController {
    partial class SystemVarsEdit {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SystemVarsEdit));
            this.Lb_List = new System.Windows.Forms.ListBox();
            this.Tb_NewString = new System.Windows.Forms.TextBox();
            this.Bt_Ok = new System.Windows.Forms.Button();
            this.Bt_SaveToFile = new System.Windows.Forms.Button();
            this.Lb_Path = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Lb_List
            // 
            this.Lb_List.FormattingEnabled = true;
            this.Lb_List.Location = new System.Drawing.Point(13, 13);
            this.Lb_List.Name = "Lb_List";
            this.Lb_List.Size = new System.Drawing.Size(353, 108);
            this.Lb_List.TabIndex = 0;
            this.Lb_List.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // Tb_NewString
            // 
            this.Tb_NewString.Location = new System.Drawing.Point(13, 127);
            this.Tb_NewString.Name = "Tb_NewString";
            this.Tb_NewString.Size = new System.Drawing.Size(261, 20);
            this.Tb_NewString.TabIndex = 1;
            this.Tb_NewString.TextChanged += new System.EventHandler(this.Tb_NewString_TextChanged);
            // 
            // Bt_Ok
            // 
            this.Bt_Ok.Enabled = false;
            this.Bt_Ok.Location = new System.Drawing.Point(281, 127);
            this.Bt_Ok.Name = "Bt_Ok";
            this.Bt_Ok.Size = new System.Drawing.Size(85, 23);
            this.Bt_Ok.TabIndex = 2;
            this.Bt_Ok.Text = "Ok";
            this.Bt_Ok.UseVisualStyleBackColor = true;
            this.Bt_Ok.Click += new System.EventHandler(this.Bt_Ok_Click);
            // 
            // Bt_SaveToFile
            // 
            this.Bt_SaveToFile.Location = new System.Drawing.Point(281, 156);
            this.Bt_SaveToFile.Name = "Bt_SaveToFile";
            this.Bt_SaveToFile.Size = new System.Drawing.Size(85, 23);
            this.Bt_SaveToFile.TabIndex = 3;
            this.Bt_SaveToFile.Text = "Save to file";
            this.Bt_SaveToFile.UseVisualStyleBackColor = true;
            this.Bt_SaveToFile.Click += new System.EventHandler(this.Bt_SaveToFile_Click);
            // 
            // Lb_Path
            // 
            this.Lb_Path.AutoSize = true;
            this.Lb_Path.Location = new System.Drawing.Point(12, 150);
            this.Lb_Path.MaximumSize = new System.Drawing.Size(250, 26);
            this.Lb_Path.Name = "Lb_Path";
            this.Lb_Path.Size = new System.Drawing.Size(28, 13);
            this.Lb_Path.TabIndex = 4;
            this.Lb_Path.Text = "XXX";
            this.Lb_Path.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SystemVarsEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(378, 187);
            this.Controls.Add(this.Lb_Path);
            this.Controls.Add(this.Bt_SaveToFile);
            this.Controls.Add(this.Bt_Ok);
            this.Controls.Add(this.Tb_NewString);
            this.Controls.Add(this.Lb_List);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SystemVarsEdit";
            this.Text = "SystemVarsEdit";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox Lb_List;
        private System.Windows.Forms.TextBox Tb_NewString;
        private System.Windows.Forms.Button Bt_Ok;
        private System.Windows.Forms.Button Bt_SaveToFile;
        private System.Windows.Forms.Label Lb_Path;
    }
}