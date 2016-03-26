using Newtonsoft.Json;
using SpeechMusicController.AppSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeechMusicController {

    internal static class Program {

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args) {
            //Delayed start
            try {
                for(int i = 0; i < args.Length; i++) {
                    if(args[i] == "-delayed" || args[i] == "/delayed") {
                        System.Threading.Thread.Sleep(Int32.Parse(args[i + 1]));
                    }
                }
            } catch { }

            //Housekeeping
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //Init settings (file)
            Settings settings = null;
            string path = Settings.FilePath;
            try {
                settings = SettingsFile.ReadSettingFile<Settings>(path);

                if(settings == null) {
                    File.Delete(path);
                    settings = new Settings(true);
                }
            } catch(JsonReaderException jre) {
                MessageBox.Show($"Invalid Json in settings file!\n{jre}");
                Environment.Exit(-1);
            } catch(Exception e) {
                MessageBox.Show($"Something went wrong while reading settings file {path}!\n{e}");
                Environment.Exit(-1);
            }

            //Set missing player/musicfolder paths
            if(string.IsNullOrEmpty(settings.GetSetting("PlayerPath") as string)) {
                MessageBox.Show("Pick player exe");
                var ofd = new OpenFileDialog() {
                    Title = "Pick player exe",
                    Filter = "Excecutable (*.exe)|*.exe"
                };
                if(ofd.ShowDialog() == DialogResult.OK) {
                    settings.SetSetting("PlayerPath", ofd.FileName);
                }
            }

            if(string.IsNullOrEmpty(settings.GetSetting("MusicFolder") as string)) {
                MessageBox.Show("Pick music folder");
                var fbd = new FolderBrowserDialog();
                if(fbd.ShowDialog() == DialogResult.OK) {
                    settings.SetSetting("MusicFolder", fbd.SelectedPath);
                }
            }

            //Run app
#if !DEBUG
            try {
#endif
            settings.WriteToDisc(false); //Make sure file exists
            Application.ApplicationExit += (sender, e) => settings.WriteToDisc(false);
            Application.Run(new MainForm(settings));
#if !DEBUG
            } catch(Exception e) {
                MessageBox.Show($"An error occured!\n{e}");
            }
#endif
        }
    }
}