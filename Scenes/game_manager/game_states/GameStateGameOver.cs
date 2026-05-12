using Godot;

[GlobalClass]
public partial class GameStateGameOver : GameState
{
    public SoundPlayer soundPlayer;
    public GameEvents gameEvents;

    public override void _EnterTree()
    {
        int winningTeamID = manager.GetWinningTeam();
        soundPlayer = GetNode<SoundPlayer>("/root/SoundPlayer");
        gameEvents = GetNode<GameEvents>("/root/GameEvents");

        soundPlayer.Play(SoundPlayer.Sound.WHISTLE);
        gameEvents.EmitGameOver(winningTeamID);
    }
}