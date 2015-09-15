# SpeechMusicController
Controll a music collection with voice commands.

(Starts minimized)

Before the program can operate set the 'MusicFolder' variable to the path of you music collection and the 'PlayerPath' variable to the player .exe (currently only AIMP3 supported). These variables can be set under the 'Settings' button.

Song information will be gathered first from mp3 header tags and secondly from the name of the song. If no mp3 header tags are found and the name is not in the format `[artist] - [title].mp3` the song will be ignored.

#### Use
Say: `Music + [Search keyword]` for a search by every tag, all found songs with matching title, artist or album will be shuffled and played.

`[Song/Artist/Album] + [Search keyword]` to only match by a certain tag.

Or use general command like `Music + [Command]`, some available commands are:
 - `collection` (shuffle play all songs)
 - `random`
 - `switch` (acts as pause/unpause)
 - `volume [up/down]`
 - `[previous]/[next]`

All commands are listed on startup in the log.
