using Godot;
using System;

public partial class BallStateFreeform : BallState
{
    private const float MaxCaptureHeight = 25f;
    private const float MagnetRadius = 30f; // â NEW - pull ball toward receiver
    private ulong timeSinceFreeform;
    private PlayerCharacter previousCarrier;
    private PlayerCharacter intendedReceiver; // â NEW

    public override void _EnterTree()
    {
        playerDetectionArea.BodyEntered += OnPlayerEnter;
        timeSinceFreeform = Time.GetTicksMsec();

        // Store passer and intended receiver
        previousCarrier = ball.Carrier;
        intendedReceiver = stateData.PassReceiver; // â NEW

        if (previousCarrier != null)
        {
            ball.AddCollisionExceptionWith(previousCarrier);
        }
    }

    public override void _ExitTree()
    {
        if (playerDetectionArea != null)
            playerDetectionArea.BodyEntered -= OnPlayerEnter;

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
            // â Don't allow passer to immediately reclaim during lock window
            if (p == previousCarrier &&
                (Time.GetTicksMsec() - timeSinceFreeform) < (ulong)stateData.LockDuration)
                return;

            ball.Carrier = p;
            ball.Carrier.gameManager.currentMatch.LastBallCarrier = p.PlayerID;
            p.ControlBall();
            TransitionState(Ball.State.CARRIED);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float floatDelta = (float)delta;

        // Enable collision detection after lock period expires
        playerDetectionArea.Monitoring =
            (Time.GetTicksMsec() - timeSinceFreeform) > (ulong)stateData.LockDuration;

        SetBallAnimationFromVelocity();

        // â IMPROVED FRICTION: Match new pass physics
        float friction = ball.Height > 1f ? 35f : 450f; // Slightly higher ground friction
        ball.Velocity = ball.Velocity.MoveToward(Vector2.Zero, friction * floatDelta);

        // â STRONGER MAGNETIC PULL toward intended receiver
        if (intendedReceiver != null && ball.Height <= MaxCaptureHeight)
        {
            float distanceToReceiver = ball.Position.DistanceTo(intendedReceiver.Position);

            // â DOUBLED MAGNET RANGE
            float magnetRadius = 65f; // Was 30f

            if (distanceToReceiver < magnetRadius)
            {
                // â MUCH STRONGER PULL (nearly guaranteed connection if close)
                Vector2 toReceiver = ball.Position.DirectionTo(intendedReceiver.Position);
                float pullStrength = (magnetRadius - distanceToReceiver) / magnetRadius;
                pullStrength = Mathf.Pow(pullStrength, 0.8f); // Gentler curve for smoothness

                // â TRIPLED PULL FORCE
                ball.Velocity += toReceiver * pullStrength * 360f * floatDelta; // Was 120f
            }
        }

        ProcessGravity(floatDelta, Ball.BOUNCINESS);
        MoveAndBounce(floatDelta);
    }

    public override bool CanAirInteract()
    {
        return true;
    }
}