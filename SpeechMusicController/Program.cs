using SpeechMusicController.AppSettings;
using System;
using System.Collections.Generic;
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

            Settings.Instance.WriteToDisc(); //Make sure file exists

            Application.ApplicationExit += (sender, e) => Settings.Instance.WriteToDisc();
#if DEBUG
            Application.Run(new MainForm());
#else
            try {
                Application.Run(new MainForm());
            } catch (Exception e) {
                System.Windows.Forms.MessageBox.Show("An error occured! " + e.ToString());
            }
#endif
        }
    }
}