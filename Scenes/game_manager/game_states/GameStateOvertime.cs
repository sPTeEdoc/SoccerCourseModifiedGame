using FunnyOldGame;
using Godot;

[GlobalClass]
public partial class GameStateOvertime : GameState
{
    public GameEvents gameEvents;
    public override void _EnterTree()
    {
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameEvents.TeamScored += OnTeamScored;
    }

    public override void _ExitTree()
    {
        if (gameEvents != null)
            gameEvents.TeamScored -= OnTeamScored;
    }


    private void OnTeamScored(string countryScoredOn)
    {
        manager.IncreaseScore(countryScoredOn);
        TransitionState(GameManager.State.GAMEOVER);
    }
}