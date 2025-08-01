using Godot;
using System;

[GlobalClass]
public partial class GameState : Node
{
    [Signal]
    public delegate void StateTransitionRequestedEventHandler(GameManager.State newState, GameStateData data);

    public GameManager manager = null;
    public GameStateData stateData = null;

    public void Setup(GameManager contextManager, GameStateData contextData)
    {
        manager = contextManager;
        stateData = contextData;
    }

    public void TransitionState(GameManager.State newState, GameStateData data = null)
    {
        if (data == null)
            data = new GameStateData();
        if (!IsInstanceValid(this))
        {
            GD.PrintErr("Trying to emit signal after disposal.");
            return;
        }

        EmitSignal(nameof(StateTransitionRequested), (int)newState, data);
    }
}