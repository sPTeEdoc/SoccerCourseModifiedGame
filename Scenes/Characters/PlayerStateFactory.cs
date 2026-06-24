using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class PlayerStateFactory : Node
{
    private readonly Dictionary<PlayerCharacter.State, Type> states = new()
    {
        { PlayerCharacter.State.BICYCLE_KICK, typeof(PlayerStateBicycleKick) },
        { PlayerCharacter.State.CELEBRATING, typeof(PlayerStateCelebrating) },
        { PlayerCharacter.State.CHEST_CONTROL, typeof(PlayerStateChestControl) },
        { PlayerCharacter.State.DIVING, typeof(PlayerStateDiving) },
        { PlayerCharacter.State.HURT, typeof(PlayerStateHurt) },
        { PlayerCharacter.State.HEADER, typeof(PlayerStateHeader) },
        { PlayerCharacter.State.MOURNING, typeof(PlayerStateMourning) },
        { PlayerCharacter.State.MOVING, typeof(PlayerStateMoving) },
        { PlayerCharacter.State.PASSING, typeof(PlayerStatePassing) },
        { PlayerCharacter.State.RECEIVING_PASS, typeof(PlayerStateReceivingPass) },
        { PlayerCharacter.State.PREPPING_SHOT, typeof(PlayerStatePreppingShot) },
        { PlayerCharacter.State.RESETING, typeof(PlayerStateReseting) },
        { PlayerCharacter.State.RECOVERING, typeof(PlayerStateRecovering) },
        { PlayerCharacter.State.SHOOTING, typeof(PlayerStateShooting) },
        { PlayerCharacter.State.TACKLING, typeof(PlayerStateTackling) },
        { PlayerCharacter.State.STANDING_TACKLE, typeof(PlayerStateStandingTackle) },
        { PlayerCharacter.State.VOLLEY_KICK, typeof(PlayerStateVolleyKick) }
    };

    public PlayerState GetFreshState(PlayerCharacter.State state, Node parent)
    {
        if (!states.ContainsKey(state))
            throw new InvalidOperationException($"State '{state}' doesn't exist!");

        var stateType = states[state];
        var newState = Activator.CreateInstance(stateType) as PlayerState;

        if (newState == null)
            throw new InvalidCastException($"Failed to instantiate {stateType.Name} as PlayerState.");

        // parent.AddChild(newState);         // 💥 add to scene tree
        newState.SetProcess(true);         // 💥 enable lifecycle
        return newState;
    }

}