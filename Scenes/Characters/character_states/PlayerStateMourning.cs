using Godot;
using System;

[GlobalClass]
public partial class PlayerStateMourning : PlayerState
{
    public GameEvents gameEvents;
    public override void _EnterTree()
    {
        GD.Print("Celebrating ENTERED: " + this.GetInstanceId());

        animationPlayer.Play("mourn");
        player.Velocity = Vector2.Zero;     
    }

    public override void _Ready()
    {
        gameEvents = GetNode<GameEvents>("/root/GameEvents");
        var callable = new Callable(this, nameof(OnTeamReset));
        if (!gameEvents.IsConnected("TeamResetEventTriggered", callable))
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
        GD.Print("Celebrating EXITED: " + this.GetInstanceId());
        if (gameEvents != null)
            gameEvents.TeamResetEventTriggered -= OnTeamReset;
    }

    public override void Cleanup()
    {
        GD.Print("Celebrating CLEANUP: " + this.GetInstanceId());
        if (gameEvents != null)
            gameEvents.TeamResetEventTriggered -= OnTeamReset;
    }

}