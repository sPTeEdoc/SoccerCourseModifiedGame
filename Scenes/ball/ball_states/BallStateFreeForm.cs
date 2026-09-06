using Godot;
using System;

public partial class BallStateFreeform : BallState
{
    private const float MaxCaptureHeight = 25f;
    private ulong timeSinceFreeform;
    private PlayerCharacter previousCarrier;

    public override void _EnterTree()
    {
        playerDetectionArea.BodyEntered += OnPlayerEnter;
        timeSinceFreeform = Time.GetTicksMsec();

        // Store and temporarily ignore the player who just passed/kicked the ball
        previousCarrier = ball.Carrier;
        if (previousCarrier != null)
        {
            ball.AddCollisionExceptionWith(previousCarrier);
        }
    }

    public override void _ExitTree()
    {
        if (playerDetectionArea != null)
            playerDetectionArea.BodyEntered -= OnPlayerEnter;

        // Clean up collision exception when leaving freeform
        if (previousCarrier != null)
        {
            ball.RemoveCollisionExceptionWith(previousCarrier);
            previousCarrier = null;
        }
    }

    private void OnPlayerEnter(Node body)
    {
        if (body is PlayerCharacter p && p.CanCarryBall() && ball.Height < MaxCaptureHeight)
        {
            // Don't allow the passer to re-claim their own pass during the lock window
            if (p == previousCarrier && (Time.GetTicksMsec() - timeSinceFreeform) < (ulong)stateData.LockDuration)
                return;

            ball.Carrier = p;
            ball.Carrier.gameManager.currentMatch.LastBallCarrier = p.playerID;
            p.ControlBall();
            TransitionState(Ball.State.CARRIED);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float floatDelta = (float)delta;

        playerDetectionArea.Monitoring =
            (Time.GetTicksMsec() - timeSinceFreeform) > (ulong)stateData.LockDuration;

        SetBallAnimationFromVelocity();

        float friction = ball.Height > 0 ? ball.FrictionAir : ball.FrictionGround;
        ball.Velocity = ball.Velocity.MoveToward(Vector2.Zero, friction * floatDelta);

        ProcessGravity(floatDelta, Ball.BOUNCINESS);
        MoveAndBounce(floatDelta);
    }

    public override bool CanAirInteract()
    {
        return true;
    }
}