using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechControl.CommandMatch {

    public static class Matcher {

        public static CommandMatchResult BestMatch(IEnumerable<string> sentence, IEnumerable<SpeechCommand> commands) {
            var matches = commands.Select(c => Match(sentence, c));

            var fullMatch = matches.FirstOrDefault(cmr => cmr.MatchType == MatchType.FullMatch);
            if(fullMatch != null) {
                return fullMatch;
            }

            var startMatches = matches.Where(cmr => cmr.MatchType == MatchType.StartMatch);
            if(startMatches.Count() > 0) {
                var bestStartMatch = startMatches.Aggregate((agg, cur) => agg.Sentence.Count() >= cur.Sentence.Count() ? agg : cur);
                if(bestStartMatch != null) {
                    return bestStartMatch;
                }
            }

            return matches.FirstOrDefault();
        }

        public static CommandMatchResult BestMatch(IEnumerable<string> sentence, params SpeechCommand[] commands) {
            return BestMatch(sentence, commands);
        }

        public static CommandMatchResult Match(IEnumerable<string> sentence, SpeechCommand command) {
            MatchType matchType = MatchType.NoMatch;
            IEnumerable<string> matchSentence = new string[0];

            for(int skip = sentence.Count() - 1; skip >= 0; skip--) {
                var curSentence = sentence.Skip(skip);

                int nrOfKeyMatches = curSentence.Zip(command.KeyWords, (sentenceWord, keyWords) => keyWords.Contains(sentenceWord)).TakeWhile(b => b).Count();
                if(nrOfKeyMatches > matchSentence.Count() && nrOfKeyMatches == curSentence.Count()) {
                    matchSentence = curSentence.Take(nrOfKeyMatches);

                    if(nrOfKeyMatches == command.KeyWords.Count) {
                        matchType = MatchType.FullMatch;
                        break;
                    } else {
                        matchType = MatchType.StartMatch;
                    }
                }
            }

            return new CommandMatchResult(matchType, matchSentence, command);
        }
    }
}