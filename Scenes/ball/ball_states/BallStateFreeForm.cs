using Godot;
using System;

public partial class BallStateFreeform : BallState
{
    private const float MaxCaptureHeight = 25f;

    private ulong timeSinceFreeform;

    public override void _EnterTree()
    {
        playerDetectionArea.BodyEntered += OnPlayerEnter;
        timeSinceFreeform = Time.GetTicksMsec();
    }

    private void OnPlayerEnter(Node body)
    {
        if (body is PlayerCharacter p && p.CanCarryBall() && ball.Height < MaxCaptureHeight)
        {
            ball.Carrier = p;
            p.ControlBall();
            TransitionState(Ball.State.CARRIED);
        }
    }

    public override void _Process(double delta)
    {
        playerDetectionArea.Monitoring =
            (Time.GetTicksMsec() - timeSinceFreeform) > (ulong)stateData.LockDuration;

        SetBallAnimationFromVelocity();

        float friction = ball.Height > 0 ? ball.FrictionAir : ball.FrictionGround;
        ball.Velocity = ball.Velocity.MoveToward(Vector2.Zero, friction * (float)delta);

        ProcessGravity((float)delta, Ball.BOUNCINESS);
        MoveAndBounce((float)delta);
    }

    public override bool CanAirInteract()
    {
        return true;
    }
}