using System;

namespace SpeechMusicController {

    internal class CommandModeTimer {
        private const long DefaultActiveTime = 6000;

        private CommandMode CurrentMode = CommandMode.None;
        private long ActiveUntil = 0;

        public static string[] ModeNames => Enum.GetNames(typeof(CommandMode));

        public CommandMode ActiveMode => IsModeActive(CurrentMode) ? CurrentMode : CommandMode.None;

        public bool NoModeActive => IsModeActive(CommandMode.None);
        public bool MusicModeActive => IsModeActive(CommandMode.Music);
        public bool SongModeActive => IsModeActive(CommandMode.Song);
        public bool ArtistModeActive => IsModeActive(CommandMode.Artist);
        public bool AlbumModeActive => IsModeActive(CommandMode.Album);

        public CommandMode GetModeByName(string name) {
            CommandMode result;
            if(Enum.TryParse(name, true, out result)) {
                return result;
            } else {
                return CommandMode.None;
            }
        }

        public void ActivateMode(CommandMode mode, long ActiveTimeMs = DefaultActiveTime) {
            CurrentMode = mode;
            ActiveUntil = Environment.TickCount + ActiveTimeMs;
        }

        public void ActivateMode(string modeName, long ActiveTimeMs = DefaultActiveTime) {
            ActivateMode(GetModeByName(modeName), ActiveTimeMs);
        }

        public bool IsModeActive(CommandMode mode) => mode == CurrentMode && ActiveUntil > Environment.TickCount;
    }

    internal enum CommandMode {
        None, Music, Song, Artist, Album
    }
}