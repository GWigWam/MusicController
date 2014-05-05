using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SpeechMusicController {
    class Program {

        static void Main(string[] args) {
            SpeechInput sp = new SpeechInput();
            sp.start();
            Console.WriteLine("Listening...\nSay 'Music', wait for beep then available commands are:\n- switch (pause/unpause)\n- random (random song)\n- <song name> (to play said song)\n\nIf command is understood tone will sound again");

            while(true) {
                Console.ReadLine();
            }
        }
    }
}
