using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpeechMusicController.Settings {

    internal static class SongRules {

        public static int NextRuleId {
            get {
                if (Rules != null && Rules.Count > 0) {
                    return Rules.Select(r => r.Id).Max() + 1;
                } else {
                    return 0;
                }
            }
        }

        private const string SongSettinsFile = "SongRules.settings";
        private static List<SongRule> Rules = new List<SongRule>();

        static SongRules() {
            if (!File.Exists(SongSettinsFile)) {
                var f = File.Create(SongSettinsFile);
                f.Dispose();
            }

            using (var sr = new StreamReader(SongSettinsFile)) {
                string line = string.Empty;
                while ((line = sr.ReadLine()) != null) {
                    SongRule res;
                    if (SongRule.TryParse(line, out res)) {
                        Rules.Add(res);
                    }
                }
            }
        }

        public static void AddRule(SongRule rule) {
            if (Rules.FirstOrDefault(sr => sr.Id == rule.Id) == null) {
                using (var sw = new StreamWriter(SongSettinsFile, true)) {
                    var line = rule.ToSettingLine();
                    sw.WriteLine(line);
                }

                Rules.Add(rule);
            }
        }

        public static void RemoveRule(int ruleId) {
            Rules.RemoveAll(sr => sr.Id == ruleId);

            File.WriteAllLines(SongSettinsFile,
                File.ReadAllLines(SongSettinsFile).Where(l => !l.StartsWith(string.Format("{0}{1}", ruleId, SongRule.Seperator))));
        }

        public static void RemoveRule(SongRule rule) {
            RemoveRule(rule.Id);
        }

        public static SongRule[] GetRules() {
            return Rules.ToArray();
        }

        public static SongRule[] GetRules(SongRuleType type) {
            return Rules.Where(r => r.Type == type).ToArray();
        }
    }
}