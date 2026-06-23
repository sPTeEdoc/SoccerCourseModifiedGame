using Godot;
using System;

[GlobalClass]
public partial class PlayerStateMourning : PlayerState
{
    public GameEvents gameEvents;
    public override void _EnterTree()
    {
        animationPlayer.Play($"{player.AnimPrefix}mourn");
        player.Velocity = Vector2.Zero;     
    }

    public override void _Ready()
    {
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        gameEvents.TeamResetEventTriggered += OnTeamReset;
    }

    private void OnTeamReset()
    {
        TransitionState(PlayerCharacter.State.RESETING,
            PlayerStateData.Build()
                .SetResetPosition(player.kickoffPosition));
    }

    public override void _ExitTree()
    {
        if (gameEvents != null)
            gameEvents.TeamResetEventTriggered -= OnTeamReset;
    }
}