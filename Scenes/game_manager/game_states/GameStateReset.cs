using Godot;

[GlobalClass]
public partial class GameStateReset : GameState
{
    public GameEvents gameEvents;

    public override void _Ready()
    {
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameEvents.CallDeferred(nameof(GameEvents.EmitReset));
        gameEvents.KickoffReady += OnKickoffReady;
    }

    public override void _EnterTree()
    {

    }

    private void OnKickoffReady()
    {
        TransitionState(GameManager.State.KICKOFF, stateData);
    }

    public override void _ExitTree()
    {
        if (gameEvents != null)
            gameEvents.KickoffReady -= OnKickoffReady;

    }
}