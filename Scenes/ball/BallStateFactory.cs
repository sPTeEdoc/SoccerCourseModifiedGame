using Godot;
using System;
using System.Collections.Generic;

public partial class BallStateFactory : Node
{
    private Dictionary<Ball.State, Type> states;

    public override void _Ready()
    {
        states = new Dictionary<Ball.State, Type>
        {
            { Ball.State.CARRIED, typeof(BallStateCarried) },
            { Ball.State.FREEFORM, typeof(BallStateFreeform) },
            { Ball.State.SHOT, typeof(BallStateShot) }
        };
    }

    public BallState GetFreshState(Ball.State state)
    {
        if (!states.ContainsKey(state))
        {
            GD.PushError($"State '{state}' doesn't exist!");
            return null;
        }

        var instance = (BallState)Activator.CreateInstance(states[state]);
        return instance;
    }
}