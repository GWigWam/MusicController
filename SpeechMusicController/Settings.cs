using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

namespace SpeechMusicController {
    class Settings {

        public static string readAIMP3Location() {
            try {
                StreamReader file = new StreamReader("AIMP3Location.settings");
                return file.ReadLine();
            } catch(Exception e) {
                MessageBox.Show("Error while looking for AIMP3's location.\nMake sure the file AIMP3Location.settings is next to exe and contains on the first line the location of AIMP3\n\n" + e.Message);
                Environment.Exit(0);
                return null;
            }
        }

        public static string readMusicLocation() {
            try {
                StreamReader file = new StreamReader("MusicLocation.settings");
                return file.ReadLine();
            } catch(Exception e) {
                MessageBox.Show("Error while looking for Music folders location.\nMake sure the file MusicLocation.settings is next to exe and contains on the first line the location of the music folder\n\n" + e.Message);
                Environment.Exit(0);
                return null;
            }
        }
    }
}
