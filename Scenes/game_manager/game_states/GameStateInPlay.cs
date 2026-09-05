using Godot;

[GlobalClass]
public partial class GameStateInPlay : GameState
{
    public GameEvents gameEvents;
    private bool isTransitioningHalf = false;

    public override void _EnterTree()
    {
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameEvents.TeamScored += OnTeamScored;
        isTransitioningHalf = false;
    }

    public override void _ExitTree()
    {
        if (gameEvents != null)
            gameEvents.TeamScored -= OnTeamScored;
    }

    public override void _Process(double delta)
    {
        if (isTransitioningHalf) return;

        manager.timeLeft -= (float)delta;

        if (manager.IsTimeUp())
        {
            isTransitioningHalf = true;
            var container = GetTree().CurrentScene.GetNodeOrNull<FullFieldActorsContainer>("FullFieldActorsContainer")
                            ?? GetParent() as FullFieldActorsContainer;

            if (container != null)
            {
                container.StartHalfOverSequence();
            }
            else
            {
                // Fallback direct signal emission
                gameEvents.EmitSignal("HalfOver");
            }
        }
    }

    private void OnTeamScored(int teamScoredOn)
    {
        // Golden Goal check: If a goal is scored in overtime (Half > 2), end match immediately
        if (manager.currentMatch.Half > 2)
        {
            manager.currentMatch.IncreaseScore(teamScoredOn);
            TransitionState(GameManager.State.GAMEOVER);
            return;
        }

        TransitionState(
            GameManager.State.SCORED,
            GameStateData.Build().SetTeamScoredOn(teamScoredOn)
        );
    }
}