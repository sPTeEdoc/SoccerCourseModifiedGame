using Godot;

[GlobalClass]
public partial class GameStateGameOver : GameState
{
    public Node soundPlayer;
    public Node gameEvents;

    public override void _EnterTree()
    {
        string countryWinner = manager.GetWinnerCountry();
        ((SoundPlayer)soundPlayer).Play(SoundPlayer.Sound.WHISTLE);
        ((GameEvents)gameEvents).EmitGameOver(countryWinner);
    }
}