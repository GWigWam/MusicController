using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpeechMusicController.Settings {

    internal static class PathSettings {
        private const string PlayerFile = "AIMP3Location.settings";
        private const string MusicFolderFile = "MusicLocation.settings";

        public static string ReadAIMP3Location() {
            try {
                StreamReader file = new StreamReader(PlayerFile);
                return file.ReadLine();
            } catch (Exception e) {
                MessageBox.Show(string.Format("Error while looking for AIMP3's location.\nMake sure the file {0} is next to exe and contains on the first line the location of AIMP3\n\n{1}", PlayerFile, e.Message));
                Environment.Exit(0);
                return null;
            }
        }

        public static string ReadMusicLocation() {
            try {
                StreamReader file = new StreamReader(MusicFolderFile);
                return file.ReadLine();
            } catch (Exception e) {
                MessageBox.Show(string.Format("Error while looking for Music folders location.\nMake sure the file {0} is next to exe and contains on the first line the location of the music folder\n\n{1}", MusicFolderFile, e.Message));
                Environment.Exit(0);
                return null;
            }
        }
    }
}