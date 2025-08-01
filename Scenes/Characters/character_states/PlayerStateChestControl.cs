using Godot;
using System;

[GlobalClass]
public partial class PlayerStateChestControl : PlayerState
{
    private const int DurationControl = 500;
    private int timeSinceControl = (int)Time.GetTicksMsec();

    public override void _EnterTree()
    {
        animationPlayer.Play("chest_control");
        player.Velocity = Vector2.Zero;
        timeSinceControl = (int)Time.GetTicksMsec();
    }

    public override void _Process(double delta)
    {
        if ((int)Time.GetTicksMsec() - timeSinceControl > DurationControl)
        {
            TransitionState(PlayerCharacter.State.MOVING);
        }
    }

    public override bool CanPass() => true;
}