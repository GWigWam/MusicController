﻿namespace SpeechMusicController {
    partial class RulesEdit {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RulesEdit));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Lb_Rules = new System.Windows.Forms.ListBox();
            this.Lb_Songs = new System.Windows.Forms.ListBox();
            this.Bt_ExcludeSong = new System.Windows.Forms.Button();
            this.Tb_Rename = new System.Windows.Forms.TextBox();
            this.Bt_RenameSong = new System.Windows.Forms.Button();
            this.Bt_DeleteRule = new System.Windows.Forms.Button();
            this.Tb_Search = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Rules:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 181);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Songs:";
            // 
            // Lb_Rules
            // 
            this.Lb_Rules.FormattingEnabled = true;
            this.Lb_Rules.Location = new System.Drawing.Point(12, 44);
            this.Lb_Rules.Name = "Lb_Rules";
            this.Lb_Rules.Size = new System.Drawing.Size(548, 134);
            this.Lb_Rules.TabIndex = 4;
            this.Lb_Rules.Click += new System.EventHandler(this.Lb_Rules_Click);
            // 
            // Lb_Songs
            // 
            this.Lb_Songs.FormattingEnabled = true;
            this.Lb_Songs.Location = new System.Drawing.Point(12, 197);
            this.Lb_Songs.Name = "Lb_Songs";
            this.Lb_Songs.Size = new System.Drawing.Size(548, 134);
            this.Lb_Songs.TabIndex = 5;
            this.Lb_Songs.Click += new System.EventHandler(this.Lb_Songs_Click);
            // 
            // Bt_ExcludeSong
            // 
            this.Bt_ExcludeSong.Enabled = false;
            this.Bt_ExcludeSong.Location = new System.Drawing.Point(235, 344);
            this.Bt_ExcludeSong.Name = "Bt_ExcludeSong";
            this.Bt_ExcludeSong.Size = new System.Drawing.Size(75, 23);
            this.Bt_ExcludeSong.TabIndex = 6;
            this.Bt_ExcludeSong.Text = "Exclude";
            this.Bt_ExcludeSong.UseVisualStyleBackColor = true;
            this.Bt_ExcludeSong.Click += new System.EventHandler(this.Bt_ExcludeSong_Click);
            // 
            // Tb_Rename
            // 
            this.Tb_Rename.Enabled = false;
            this.Tb_Rename.Location = new System.Drawing.Point(397, 346);
            this.Tb_Rename.Name = "Tb_Rename";
            this.Tb_Rename.Size = new System.Drawing.Size(163, 20);
            this.Tb_Rename.TabIndex = 7;
            this.Tb_Rename.Click += new System.EventHandler(this.Tb_Rename_Click);
            // 
            // Bt_RenameSong
            // 
            this.Bt_RenameSong.Enabled = false;
            this.Bt_RenameSong.Location = new System.Drawing.Point(316, 344);
            this.Bt_RenameSong.Name = "Bt_RenameSong";
            this.Bt_RenameSong.Size = new System.Drawing.Size(75, 23);
            this.Bt_RenameSong.TabIndex = 8;
            this.Bt_RenameSong.Text = "Rename";
            this.Bt_RenameSong.UseVisualStyleBackColor = true;
            this.Bt_RenameSong.Click += new System.EventHandler(this.Bt_RenameSong_Click);
            // 
            // Bt_DeleteRule
            // 
            this.Bt_DeleteRule.Enabled = false;
            this.Bt_DeleteRule.Location = new System.Drawing.Point(12, 344);
            this.Bt_DeleteRule.Name = "Bt_DeleteRule";
            this.Bt_DeleteRule.Size = new System.Drawing.Size(75, 23);
            this.Bt_DeleteRule.TabIndex = 10;
            this.Bt_DeleteRule.Text = "Delete Rule";
            this.Bt_DeleteRule.UseVisualStyleBackColor = true;
            this.Bt_DeleteRule.Click += new System.EventHandler(this.Bt_DeleteRule_Click);
            // 
            // Tb_Search
            // 
            this.Tb_Search.Location = new System.Drawing.Point(55, 6);
            this.Tb_Search.Name = "Tb_Search";
            this.Tb_Search.Size = new System.Drawing.Size(505, 20);
            this.Tb_Search.TabIndex = 11;
            this.Tb_Search.TextChanged += new System.EventHandler(this.Tb_Search_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Search:";
            // 
            // RulesEdit
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(572, 376);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Tb_Search);
            this.Controls.Add(this.Bt_DeleteRule);
            this.Controls.Add(this.Bt_RenameSong);
            this.Controls.Add(this.Tb_Rename);
            this.Controls.Add(this.Bt_ExcludeSong);
            this.Controls.Add(this.Lb_Songs);
            this.Controls.Add(this.Lb_Rules);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RulesEdit";
            this.Text = "RulesEdit";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RulesEdit_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox Lb_Rules;
        private System.Windows.Forms.ListBox Lb_Songs;
        private System.Windows.Forms.Button Bt_ExcludeSong;
        private System.Windows.Forms.TextBox Tb_Rename;
        private System.Windows.Forms.Button Bt_RenameSong;
        private System.Windows.Forms.Button Bt_DeleteRule;
        private System.Windows.Forms.TextBox Tb_Search;
        private System.Windows.Forms.Label label3;

    }
}