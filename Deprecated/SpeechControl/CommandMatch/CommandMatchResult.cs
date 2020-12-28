using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechControl.CommandMatch {

    public class CommandMatchResult {
        public MatchType MatchType { get; }

        public string[] Sentence { get; }

        public SpeechCommand Command { get; }

        public CommandMatchResult(MatchType type, IEnumerable<string> sentence, SpeechCommand command) {
            MatchType = type;
            Sentence = sentence.ToArray();
            Command = command;
        }

        public IEnumerable<string> Execute() {
            return Command.ExecuteCommand(Sentence);
        }
    }
}