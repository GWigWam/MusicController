using SpeechMusicController.AppSettings;
using SpeechMusicController.AppSettings.Model;
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
        private SongRules Rules;
        private MusicList AllMusic;

        public RulesEdit(SongRules rules, MusicList allMusic) {
            if(rules != null) {
                Rules = rules;
            } else {
                throw new ArgumentNullException(nameof(rules));
            }

            if(allMusic != null) {
                AllMusic = allMusic;
            } else {
                throw new ArgumentNullException(nameof(allMusic));
            }

            InitializeComponent();
            SetupLists();
        }

        private void SetupLists() {
            FillRuleList();
            FillSongList();
        }

        private void FillRuleList() {
            Lb_Rules.Items.Clear();
            IEnumerable<SongRule> allRules = Rules.GetSongRules(true, true);
            var searchWord = Tb_Search.Text;

            if(!string.IsNullOrWhiteSpace(searchWord)) {
                allRules = allRules.Where(r => r.ToString().ToLower().Contains(searchWord.ToLower()));
            }

            foreach(var rule in allRules) {
                Lb_Rules.Items.Add(rule);
            }
        }

        private void FillSongList() {
            Lb_Songs.Items.Clear();

            IEnumerable<Song> songList = AllMusic.ActiveSongs.OrderBy(s => s.Title);
            var searchWord = Tb_Search.Text;

            if(!string.IsNullOrWhiteSpace(searchWord)) {
                songList = songList.Where(s => s.ToString().ToLower().Contains(searchWord.ToLower()));
            }

            foreach(var song in songList) {
                Lb_Songs.Items.Add(song);
            }
        }

        private void Tb_Rename_Click(object sender, EventArgs e) => Tb_Rename.Text = string.Empty;

        private void Bt_DeleteRule_Click(object sender, EventArgs e) {
            SongRule selected = (Lb_Rules.SelectedItem as SongRule);

            if(selected != null) {
                Rules.RemoveSongRule(selected, false);

                int index = Lb_Rules.SelectedIndex;
                SetupLists();

                try {
                    Lb_Rules.SelectedIndex = index;
                } catch(ArgumentOutOfRangeException) { }
                Lb_Rules.Focus();
            }
        }

        private void Bt_ExcludeSong_Click(object sender, EventArgs e) {
            if(Lb_Songs.SelectedItem is Song) {
                Song selected = (Song)Lb_Songs.SelectedItem;
                Rules.AddSongRule(new ExcludeRule(selected.Attributes), false);

                int index = Lb_Songs.SelectedIndex;
                SetupLists();
                try {
                    Lb_Songs.SelectedIndex = index;
                } catch(ArgumentOutOfRangeException) { }
                Lb_Songs.Focus();
            }
        }

        private void Bt_RenameSong_Click(object sender, EventArgs e) {
            if(!string.IsNullOrWhiteSpace(Tb_Rename.Text)) {
                if(Lb_Songs.SelectedItem is Song) {
                    Song selected = (Song)Lb_Songs.SelectedItem;

                    Rules.AddSongRule(new NameChangeRule(selected.Attributes, Tb_Rename.Text.Trim()), false);

                    int index = Lb_Songs.SelectedIndex;
                    SetupLists();
                    try {
                        Lb_Songs.SelectedIndex = index;
                    } catch(ArgumentOutOfRangeException) { }
                    Lb_Songs.Focus();
                    Tb_Rename.Text = string.Empty;
                } else {
                    Tb_Rename.Text = "New Name!";
                }
            }
        }

        private void Lb_Rules_Click(object sender, EventArgs e) {
            if(Lb_Rules.SelectedIndex >= 0) {
                Lb_Songs.ClearSelected();

                Bt_DeleteRule.Enabled = true;
                Bt_ExcludeSong.Enabled = false;
                Bt_RenameSong.Enabled = false;
                Tb_Rename.Enabled = false;
            }
        }

        private void Lb_Songs_Click(object sender, EventArgs e) {
            if(Lb_Songs.SelectedIndex >= 0) {
                Lb_Rules.ClearSelected();

                Bt_DeleteRule.Enabled = false;
                Bt_ExcludeSong.Enabled = true;
                Bt_RenameSong.Enabled = true;
                Tb_Rename.Enabled = true;
            }
        }

        private void Tb_Search_TextChanged(object sender, EventArgs e) => SetupLists();

        private void RulesEdit_FormClosing(object sender, FormClosingEventArgs e) {
            Rules.TriggerOnChange();
        }
    }
}