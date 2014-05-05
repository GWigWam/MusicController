using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechMusicController {
    class Settings {

        public static string AIMP3Location {
            get {
                return @"F:\Program Files (x86)\AIMP3\AIMP3.exe";
            }
        }

        public static string MusicLocation {
            get {
                return @"F:\zooi\overige\muziek\";
            }
        }
    }
}
