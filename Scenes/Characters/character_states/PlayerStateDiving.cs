using Godot;
using System;

[GlobalClass]
public partial class PlayerStateDiving : PlayerState
{
    private const int DURATION_DIVE = 500;
    private int timeStartDive = (int)Time.GetTicksMsec();

    public override void _EnterTree()
    {
        // Dive directly toward the ball's dynamic trajectory or current position rather than the goal-line rail
        Vector2 targetDive = ball.Position;
        Vector2 direction = player.Position.DirectionTo(targetDive);

        // Select side-scrolling dive animations based on vertical launch direction
        if (direction.Y > 0)
        {
            animationPlayer.Play($"{player.AnimPrefix}dive_one_hand"); // Diving Low/Down
        }
        else
        {
            animationPlayer.Play($"{player.AnimPrefix}dive_two_hands"); // Diving High/Up
        }

        // Give the goalkeeper an explosive speed boost during the dive execution
        // This provides that sharp, responsive arcade reflex timing
        player.Velocity = direction * (player.speed * 1.6f);
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