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


    private void OnTeamScored(int teamScoredOn)
    {
        manager.IncreaseScore(teamScoredOn);
        TransitionState(GameManager.State.GAMEOVER);
    }
}