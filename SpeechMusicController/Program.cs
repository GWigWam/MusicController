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
            Console.WriteLine("started");

            //Player p = new Player();
            //p.play("\"F:\\zooi\\overige\\Muziek\\Muse - Uprising.mp3\"");
            
            Console.ReadKey();
        }
    }
}
