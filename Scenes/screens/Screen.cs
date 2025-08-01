using Godot;
using System;

public partial class Screen : Node
{
    [Signal]
    public delegate void ScreenTransitionRequestedEventHandler(SoccerGame.ScreenType newScreen, ScreenData data);

    [Export] public MusicPlayer.Music Music { get; set; }

    protected SoccerGame game;
    protected ScreenData screenData;
    public MusicPlayer musicPlayer;

    public override void _EnterTree()
    {
        musicPlayer = GetNode<MusicPlayer>("/root/MusicPlayer");
        musicPlayer.PlayMusic(Music);
    }

    public void Setup(SoccerGame contextGame, ScreenData contextData)
    {
        game = contextGame;
        screenData = contextData;
    }

    protected void TransitionScreen(SoccerGame.ScreenType newScreen, ScreenData data = null)
    {
        EmitSignal(nameof(ScreenTransitionRequested), (Variant)(int)newScreen, data ?? ScreenData.Build());
    }
}