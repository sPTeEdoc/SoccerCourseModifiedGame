using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class GameStateFactory : GodotObject
{
    private readonly Dictionary<GameManager.State, Type> states = new()
    {
        { GameManager.State.GAMEOVER, typeof(GameStateGameOver) },
        { GameManager.State.IN_PLAY, typeof(GameStateInPlay) },
        { GameManager.State.KICKOFF, typeof(GameStateKickoff) },
        { GameManager.State.OVERTIME, typeof(GameStateOvertime) },
        { GameManager.State.RESET, typeof(GameStateReset) },
        { GameManager.State.SCORED, typeof(GameStateScored) }
    };

    public GameState GetFreshState(GameManager.State state)
    {
        if (!states.ContainsKey(state))
            throw new InvalidOperationException($"State {state} does not exist in factory.");

        var stateType = states[state];
        var instance = Activator.CreateInstance(stateType) as GameState;

        if (instance == null)
            throw new InvalidCastException($"Failed to instantiate {stateType.Name} as GameState.");

        return instance;
    }

}