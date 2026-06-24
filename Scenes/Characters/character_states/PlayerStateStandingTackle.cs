using Godot;
using System;

[GlobalClass]
public partial class PlayerStateStandingTackle : PlayerState
{
    private const int JAB_DURATION = 120; 
    private const int TOTAL_DURATION = 220; 
    private int startTime;
    private bool checkExecuted = false;
    private Vector2 lockedHeading;

    public override void _EnterTree()
    {
        startTime = (int)Time.GetTicksMsec();
        checkExecuted = false;
        
        // HARD LOCK DIRECTION: Face the ball instantly so they don't poke sideways or backwards
        Vector2 dirToBall = player.Position.DirectionTo(ball.Position);
        player.heading = dirToBall.Normalized();
        lockedHeading = player.heading;

        animationPlayer.Play($"{player.AnimPrefix}trap_ball");
        
        // Kill momentum so they plant their feet
        player.Velocity = Vector2.Zero; 
    }

    public override void _Process(double delta)
    {
        int elapsed = (int)Time.GetTicksMsec() - startTime;

        // Keep the heading completely locked during the physical animation frame
        player.heading = lockedHeading;

        if (!checkExecuted && elapsed >= JAB_DURATION)
        {
            checkExecuted = true;
            EvaluateStandingChallenge();
        }

        if (elapsed >= TOTAL_DURATION)
        {
            TransitionState(PlayerCharacter.State.MOVING);
        }
    }

    private void EvaluateStandingChallenge()
    {
        if (ball.Carrier != null && ball.Carrier.teamID != player.teamID)
        {
            if (player.Position.DistanceTo(ball.Position) < 22f) // Slightly boosted radius
            {
                ball.Carrier = null;
                
                // Pop it directly where the defender was facing
                Vector2 popDir = lockedHeading;
                ball.Position += popDir * 6f;
                ball.Velocity = popDir * 180f;
                ball.SwitchState(Ball.State.FREEFORM, BallStateData.Build().SetLockDuration(150));
            }
        }
    }
}