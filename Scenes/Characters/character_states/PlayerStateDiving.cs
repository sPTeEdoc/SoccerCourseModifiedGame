using Godot;
using System;

[GlobalClass]
public partial class PlayerStateDiving : PlayerState
{
    private const int DURATION_DIVE = 500;
    private int timeStartDive = (int)Time.GetTicksMsec();

    public override void _EnterTree()
    {
        Vector2 targetDive = new Vector2(player.spawnPosition.X, ball.Position.Y);
        Vector2 direction = player.Position.DirectionTo(targetDive);

        if (direction.Y > 0)
        {
            animationPlayer.Play("dive_down");
        }
        else
        {
            animationPlayer.Play("dive_up");
        }

        player.Velocity = direction * player.speed;
        timeStartDive = (int)Time.GetTicksMsec();
    }

    public override void _Process(double delta)
    {
        if ((int)Time.GetTicksMsec() - timeStartDive > DURATION_DIVE)
        {
            TransitionState(PlayerCharacter.State.RECOVERING);
        }
    }
}