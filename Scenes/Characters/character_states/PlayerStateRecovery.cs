using Godot;
using System;

[GlobalClass]
public partial class PlayerStateRecovering : PlayerState
{
    private const int DURATION_RECOVERY = 500;
    private int timeStartRecovery = (int)Time.GetTicksMsec();

    public override void _EnterTree()
    {
        timeStartRecovery = (int)Time.GetTicksMsec();
        player.Velocity = Vector2.Zero;
        animationPlayer.Play($"{player.AnimPrefix}recover");
    }

    public override void _Process(double delta)
    {
        if ((int)Time.GetTicksMsec() - timeStartRecovery > DURATION_RECOVERY)
        {
            TransitionState(PlayerCharacter.State.MOVING);
        }
    }
}