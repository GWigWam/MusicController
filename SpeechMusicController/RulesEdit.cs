using SpeechMusicController.Settings;
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

    public partial class RulesEdit : Form {

        public RulesEdit() {
            InitializeComponent();
            SetupLists();
        }

        private void SetupLists() {
            SetupRuleList();
            SetupSongList();
        }

        private List<int> RuleIdList;

        private void SetupRuleList() {
            Lb_Rules.Items.Clear();
            RuleIdList = new List<int>();
            foreach (var rule in SongRules.GetRules()) {
                RuleIdList.Add(rule.Id);
                if (rule.Type == SongRuleType.Exclude) {
                    Lb_Rules.Items.Add(string.Format("{0}: {1}", rule.Type, MusicList.GetInternalSongByHash(rule.SongHashCode).ToString()));
                } else if (rule.Type == SongRuleType.NameChange) {
                    var tmp = string.Format("{0}: {1} --> {2}", rule.Type, MusicList.GetInternalSongByHash(rule.SongHashCode).ToString(), rule.Parameters);
                    Lb_Rules.Items.Add(tmp);
                }
            }
        }

        private List<int> SongHashCodes;

        private void SetupSongList() {
            Lb_Songs.Items.Clear();
            SongHashCodes = new List<int>();
            foreach (var song in MusicList.ActiveSongs.OrderBy(s => s.Title)) {
                Lb_Songs.Items.Add(song);
                SongHashCodes.Add(song.GetHashCode());
            }
        }

        private void DeleteRule(int ruleId) {
            SongRules.RemoveRule(ruleId);
            SetupRuleList();
        }

        //Regions
        private void Tb_Rename_Click(object sender, EventArgs e) {
            Tb_Rename.Text = string.Empty;
        }

        private void Bt_DeleteRule_Click(object sender, EventArgs e) {
            int index = Lb_Rules.SelectedIndex;
            int ruleId = RuleIdList[index];
            SongRules.RemoveRule(ruleId);
            SetupLists();

            try {
                Lb_Rules.SelectedIndex = index;
            } catch (ArgumentOutOfRangeException) { }
            Lb_Rules.Focus();
        }

        private void Bt_ExcludeSong_Click(object sender, EventArgs e) {
            int index = Lb_Songs.SelectedIndex;
            int songHash = SongHashCodes[index];
            var newRule = SongRule.newExcludeRule(songHash);
            SongRules.AddRule(newRule);
            SetupLists();
            try {
                Lb_Songs.SelectedIndex = index;
            } catch (ArgumentOutOfRangeException) { }
            Lb_Songs.Focus();
        }

        private void Bt_RenameSong_Click(object sender, EventArgs e) {
            if (!string.IsNullOrWhiteSpace(Tb_Rename.Text)) {
                int index = Lb_Songs.SelectedIndex;
                int songHash = SongHashCodes[index];
                var newRule = SongRule.newNameChangeRule(songHash, Tb_Rename.Text);
                SongRules.AddRule(newRule);
                SetupLists();
                try {
                    Lb_Songs.SelectedIndex = index;
                } catch (ArgumentOutOfRangeException) { }
                Lb_Songs.Focus();
                Tb_Rename.Text = string.Empty;
            } else {
                Tb_Rename.Text = "New Name!";
            }
        }

        private void Lb_Rules_Click(object sender, EventArgs e) {
            if (Lb_Rules.SelectedIndex >= 0) {
                Lb_Songs.ClearSelected();

                Bt_DeleteRule.Enabled = true;
                Bt_ExcludeSong.Enabled = false;
                Bt_RenameSong.Enabled = false;
                Tb_Rename.Enabled = false;
            }
        }

        private void Lb_Songs_Click(object sender, EventArgs e) {
            if (Lb_Songs.SelectedIndex >= 0) {
                Lb_Rules.ClearSelected();

                Bt_DeleteRule.Enabled = false;
                Bt_ExcludeSong.Enabled = true;
                Bt_RenameSong.Enabled = true;
                Tb_Rename.Enabled = true;
            }
        }

        private void RulesEdit_FormClosed(object sender, FormClosedEventArgs e) {
            SpeechInput.LoadGrammar();
        }
    }
}