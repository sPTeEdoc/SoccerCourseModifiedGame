using Godot;
using System;

[GlobalClass]
public partial class PlayerStateMourning : PlayerState
{
    public GameEvents gameEvents;
    public override void _EnterTree()
    {
        animationPlayer.Play("mourn");
        player.Velocity = Vector2.Zero;
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameEvents.TeamResetEventTriggered += OnTeamReset;
    }

    private void OnTeamReset()
    {
        TransitionState(PlayerCharacter.State.RESETING, 
            PlayerStateData.Build()
                .SetResetPosition(player.kickoffPosition));
    }
}