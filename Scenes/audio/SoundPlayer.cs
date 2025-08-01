using Godot;
using System.Collections.Generic;

public partial class SoundPlayer : Node
{
    public enum Sound { BOUNCE, HURT, PASS, POWERSHOT, SHOT, TACKLING, UI_NAV, UI_SELECT, WHISTLE }

    private const int NB_CHANNELS = 4;
    private static readonly Dictionary<Sound, AudioStream> SFX_MAP = new Dictionary<Sound, AudioStream>
    {
        { Sound.BOUNCE, GD.Load<AudioStream>("res://assets/sfx/bounce.wav") },
        { Sound.HURT, GD.Load<AudioStream>("res://assets/sfx/hurt.wav") },
        { Sound.PASS, GD.Load<AudioStream>("res://assets/sfx/pass.wav") },
        { Sound.POWERSHOT, GD.Load<AudioStream>("res://assets/sfx/power-shot.wav") },
        { Sound.SHOT, GD.Load<AudioStream>("res://assets/sfx/shoot.wav") },
        { Sound.TACKLING, GD.Load<AudioStream>("res://assets/sfx/tackle.wav") },
        { Sound.UI_NAV, GD.Load<AudioStream>("res://assets/sfx/ui-navigate.wav") },
        { Sound.UI_SELECT, GD.Load<AudioStream>("res://assets/sfx/ui-select.wav") },
        { Sound.WHISTLE, GD.Load<AudioStream>("res://assets/sfx/whistle.wav") }
    };

    private List<AudioStreamPlayer> streamPlayers = new List<AudioStreamPlayer>();

    public override void _Ready()
    {
        for (int i = 0; i < NB_CHANNELS; i++)
        {
            var streamPlayer = new AudioStreamPlayer();
            streamPlayers.Add(streamPlayer);
            AddChild(streamPlayer);
        }
    }

    public void Play(Sound sound)
    {
        var streamPlayer = FindFirstAvailablePlayer();
        if (streamPlayer != null)
        {
            streamPlayer.Stream = SFX_MAP[sound];
            streamPlayer.Play();
        }
    }

    private AudioStreamPlayer FindFirstAvailablePlayer()
    {
        foreach (var streamPlayer in streamPlayers)
        {
            if (!streamPlayer.Playing)
            {
                return streamPlayer;
            }
        }
        return null;
    }
}