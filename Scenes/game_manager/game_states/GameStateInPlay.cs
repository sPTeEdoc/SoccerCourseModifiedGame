using Godot;

[GlobalClass]
public partial class GameStateInPlay : GameState
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


    public override void _Process(double delta)
    {
        manager.timeLeft -= (float)delta;

        if (manager.IsTimeUp())
        {
            if (manager.currentMatch.IsTied())
            {
                TransitionState(GameManager.State.OVERTIME);
            }
            else
            {
                TransitionState(GameManager.State.GAMEOVER);
            }
        }
    }

    private void OnTeamScored(int teamScoredOn)
    {
        TransitionState(
            GameManager.State.SCORED,
            GameStateData.Build().SetTeamScoredOn(teamScoredOn)
        );
    }
}