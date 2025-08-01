using Godot;
using System.Collections.Generic;

public partial class MusicPlayer : AudioStreamPlayer
{
    public enum Music { NONE, GAMEPLAY, MENU, TOURNAMENT, WIN }

    private static readonly Dictionary<Music, AudioStream> MUSIC_MAP = new Dictionary<Music, AudioStream>
    {
        { Music.GAMEPLAY, GD.Load<AudioStream>("res://assets/music/gameplay.mp3") },
        { Music.MENU, GD.Load<AudioStream>("res://assets/music/menu.mp3") },
        { Music.TOURNAMENT, GD.Load<AudioStream>("res://assets/music/tournament.mp3") },
        { Music.WIN, GD.Load<AudioStream>("res://assets/music/win.mp3") },
    };

    private Music current_music = Music.NONE;

    public override void _Ready()
    {
        ProcessMode = Node.ProcessModeEnum.Always;
    }

    public void PlayMusic(Music music)
    {
        if (music != current_music && MUSIC_MAP.ContainsKey(music))
        {
            Stream = MUSIC_MAP[music];
            current_music = music;
            Play();
        }
    }
}