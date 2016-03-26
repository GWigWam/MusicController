using System;
using System.Collections;
using System.Collections.Generic;

namespace SpeechMusicController {

    internal class SpeechCommand {

        public string Keyword {
            get;
        }

        public Action Procedure {
            get;
        }

        public SpeechCommand(string keyword, Action procedure) {
            Keyword = keyword;
            Procedure = procedure ?? (() => { /*Do nothing*/ });
        }
    }
}