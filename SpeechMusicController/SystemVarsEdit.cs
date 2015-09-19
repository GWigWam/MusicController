using SpeechMusicController.AppSettings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeechMusicController {

    public partial class SystemVarsEdit : Form {
        private Settings AppSettings;

        public SystemVarsEdit(Settings settings) {
            if(settings != null) {
                AppSettings = settings;
            } else {
                throw new ArgumentNullException(nameof(settings));
            }
            InitializeComponent();

            Lb_Path.Text = AppSettings.FullFilePath;

            Lb_List.Items.AddRange(AppSettings.GetAllSettingNames());
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) {
            string key = (string)Lb_List.SelectedItem;
            string value = AppSettings.GetSetting(key)?.ToString() ?? string.Empty;

            Tb_NewString.Text = value;
        }

        private void Bt_Ok_Click(object sender, EventArgs e) {
            try {
                if(Lb_List.SelectedItem as string != null && !string.IsNullOrEmpty(Tb_NewString.Text)) {
                    string key = (string)Lb_List.SelectedItem;

                    Type desiredType = AppSettings.GetSetting(key).GetType();
                    string userIn = Tb_NewString.Text.Trim();
                    var converted = Convert.ChangeType(userIn, desiredType);

                    AppSettings.SetSetting(key, converted);

                    Bt_Ok.Enabled = false;
                }
            } catch(Exception ex) {
                MessageBox.Show($"An error occured\n{ex}");
            }
        }

        private void Bt_SaveToFile_Click(object sender, EventArgs e) => AppSettings.WriteToDisc(true);

        private void Tb_NewString_TextChanged(object sender, EventArgs e) {
            if(Tb_NewString.Text != (AppSettings.GetSetting((string)Lb_List.SelectedItem)?.ToString() ?? string.Empty)) {
                Bt_Ok.Enabled = true;
            } else {
                Bt_Ok.Enabled = false;
            }
        }
    }
}