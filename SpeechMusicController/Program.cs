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
            try {
                for(int i = 0; i < args.Length; i++) {
                    if(args[i] == "-delayed" || args[i] == "/delayed") {
                        System.Threading.Thread.Sleep(Int32.Parse(args[i + 1]));
                    }
                }
            } catch { }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Settings settings = null;
            string path = Settings.FilePath;
            try {
                settings = SettingsFile.ReadSettingFile<Settings>(path);

                if(settings == null) {
                    File.Delete(path);
                    settings = new Settings();
                }
            } catch(JsonReaderException jre) {
                MessageBox.Show($"Invalid Json in settings file!\n{jre}");
                Environment.Exit(-1);
            } catch(Exception e) {
                MessageBox.Show($"Something went wrong while reading settings file {path}!\n{e}");
                File.Delete(path);
                settings = new Settings();
            }

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